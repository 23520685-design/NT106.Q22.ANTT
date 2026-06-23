using Google.Cloud.Firestore;
using MiniSocialApp.Offline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniSocialApp.Services
{
    public class PostService
    {
        private readonly FirestoreDb _db;
        private readonly StorageService _storageService;
        private readonly LocalPostCacheService _postCache;

        public PostService(FirestoreContext context)
        {
            _db = context.Db;
            _storageService = new StorageService();

            LocalDatabase localDatabase =
                new LocalDatabase();

            _postCache =
                new LocalPostCacheService(localDatabase);
        }

        public PostService(
            FirestoreContext context,
            StorageService storageService)
        {
            _db = context.Db;

            _storageService = storageService
                ?? throw new ArgumentNullException(
                    nameof(storageService));

            LocalDatabase localDatabase =
                new LocalDatabase();

            _postCache =
                new LocalPostCacheService(localDatabase);
        }

        // =====================================================
        // CREATE POST
        // =====================================================

        public async Task<string> CreatePost(
            string content,
            string imagePath,
            string visibility = "public")
        {
            if (string.IsNullOrWhiteSpace(content) &&
                string.IsNullOrWhiteSpace(imagePath))
            {
                throw new Exception(
                    "Bài viết phải có nội dung hoặc hình ảnh.");
            }

            var userDict =
                CurrentUserStore.User
                    as Dictionary<string, object>;

            if (userDict == null)
            {
                throw new Exception(
                    "Người dùng chưa đăng nhập.");
            }

            string userId =
                userDict.ContainsKey("userId")
                    ? Convert.ToString(
                        userDict["userId"])
                    : null;

            string userName =
                userDict.ContainsKey("userName")
                    ? Convert.ToString(
                        userDict["userName"])
                    : null;

            string avatar =
                userDict.ContainsKey("avatar")
                    ? Convert.ToString(
                        userDict["avatar"])
                    : "";

            if (string.IsNullOrWhiteSpace(userId) ||
                string.IsNullOrWhiteSpace(userName))
            {
                throw new Exception(
                    "Thông tin người dùng không hợp lệ.");
            }

            if (visibility != "public" &&
                visibility != "followers" &&
                visibility != "private")
            {
                visibility = "public";
            }

            string mediaUrl = null;

            if (!string.IsNullOrWhiteSpace(imagePath))
            {
                mediaUrl =
                    await _storageService.UploadImage(
                        imagePath);
            }

            var post =
                new Dictionary<string, object>
                {
                    {
                        "content",
                        content != null
                            ? content.Trim()
                            : ""
                    },
                    {
                        "mediaUrl",
                        mediaUrl
                    },
                    {
                        "userId",
                        userId
                    },
                    {
                        "userName",
                        userName
                    },
                    {
                        "avatar",
                        avatar
                    },
                    {
                        "visibility",
                        visibility
                    },
                    {
                        "likeCount",
                        0
                    },
                    {
                        "commentCount",
                        0
                    },
                    {
                        "createdAt",
                        Timestamp.GetCurrentTimestamp()
                    }
                };

            DocumentReference docRef =
                await _db
                    .Collection("posts")
                    .AddAsync(post);

            return docRef.Id;
        }

        // =====================================================
        // GET FEED
        // =====================================================

        public async Task<
            List<Dictionary<string, object>>>
            GetFeed()
        {
            string currentUserId =
                GetCurrentUserId();

            try
            {
                List<Dictionary<string, object>>
                    remotePosts =
                        await GetFeedFromRemote();

                MarkPostsAsRemote(remotePosts);

                TrySaveFeedToCache(
                    currentUserId,
                    remotePosts);

                return remotePosts;
            }
            catch (Exception remoteException)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Không thể tải feed từ Firestore: " +
                    remoteException.Message);

                List<Dictionary<string, object>>
                    cachedPosts =
                        TryGetCachedFeed(
                            currentUserId);

                if (cachedPosts.Count > 0)
                {
                    return cachedPosts;
                }

                throw new Exception(
                    "Không thể tải bảng tin và chưa có dữ liệu ngoại tuyến.",
                    remoteException);
            }
        }

        private async Task<
            List<Dictionary<string, object>>>
            GetFeedFromRemote()
        {
            QuerySnapshot snapshot =
                await _db
                    .Collection("posts")
                    .WhereEqualTo(
                        "visibility",
                        "public")
                    .OrderByDescending(
                        "createdAt")
                    .Limit(50)
                    .GetSnapshotAsync();

            var userDict =
                CurrentUserStore.User
                    as Dictionary<string, object>;

            string currentUserId =
                userDict != null &&
                userDict.ContainsKey("userId")
                    ? userDict["userId"]
                        ?.ToString()
                    : "";

            var allPosts =
                snapshot.Documents
                    .Select(doc =>
                    {
                        Dictionary<string, object>
                            data =
                                doc.ToDictionary();

                        data["postId"] =
                            doc.Id;

                        if (data.ContainsKey(
                                "createdAt") &&
                            data["createdAt"]
                                is Timestamp timestamp)
                        {
                            data["createdAt"] =
                                timestamp
                                    .ToDateTime()
                                    .ToUniversalTime();
                        }

                        return data;
                    })
                    .ToList();

            // Ưu tiên 5 bài mới nhất.
            List<Dictionary<string, object>>
                newest =
                    allPosts
                        .Take(5)
                        .ToList();

            // Các bài cũ được random ổn định theo user.
            int seed =
                !string.IsNullOrEmpty(
                    currentUserId)
                    ? currentUserId
                        .GetHashCode()
                    : Environment.TickCount;

            Random random =
                new Random(seed);

            List<Dictionary<string, object>>
                randomizedOlderPosts =
                    allPosts
                        .Skip(5)
                        .OrderBy(
                            post =>
                                random.Next())
                        .Take(15)
                        .ToList();

            List<Dictionary<string, object>>
                posts =
                    newest
                        .Concat(
                            randomizedOlderPosts)
                        .ToList();

            // Lấy bài gốc cho các bài share.
            await FetchOriginalPostsAsync(
                posts);

            // Lấy trạng thái like của user hiện tại.
            if (!string.IsNullOrEmpty(
                    currentUserId))
            {
                IEnumerable<
                    Task<Dictionary<string, object>>>
                    likeTasks =
                        posts.Select(
                            async post =>
                            {
                                string postId =
                                    post["postId"]
                                        ?.ToString();

                                DocumentSnapshot likeSnapshot =
                                    await _db
                                        .Collection("posts")
                                        .Document(postId)
                                        .Collection("likes")
                                        .Document(
                                            currentUserId)
                                        .GetSnapshotAsync();

                                post["isLiked"] =
                                    likeSnapshot.Exists;

                                return post;
                            });

                return (
                    await Task.WhenAll(
                        likeTasks)
                ).ToList();
            }

            foreach (
                Dictionary<string, object>
                post in posts)
            {
                post["isLiked"] = false;
            }

            return posts;
        }

        // =====================================================
        // GET USER POSTS
        // =====================================================

        public async Task<
            List<Dictionary<string, object>>>
            GetUserPosts(string userId)
        {
            if (string.IsNullOrWhiteSpace(
                    userId))
            {
                return new List<
                    Dictionary<string, object>>();
            }

            QuerySnapshot snapshot =
                await _db
                    .Collection("posts")
                    .WhereEqualTo(
                        "userId",
                        userId)
                    .OrderByDescending(
                        "createdAt")
                    .Limit(50)
                    .GetSnapshotAsync();

            var currentUserDictionary =
                CurrentUserStore.User
                    as Dictionary<string, object>;

            string currentUserId =
                currentUserDictionary != null &&
                currentUserDictionary.ContainsKey(
                    "userId")
                    ? currentUserDictionary["userId"]
                        ?.ToString()
                    : "";

            List<Dictionary<string, object>>
                posts =
                    snapshot.Documents
                        .Select(doc =>
                        {
                            Dictionary<string, object>
                                data =
                                    doc.ToDictionary();

                            data["postId"] =
                                doc.Id;

                            if (data.ContainsKey(
                                    "createdAt") &&
                                data["createdAt"]
                                    is Timestamp timestamp)
                            {
                                data["createdAt"] =
                                    timestamp
                                        .ToDateTime()
                                        .ToUniversalTime();
                            }

                            return data;
                        })
                        .ToList();

            await FetchOriginalPostsAsync(
                posts);

            if (!string.IsNullOrEmpty(
                    currentUserId))
            {
                IEnumerable<
                    Task<Dictionary<string, object>>>
                    likeTasks =
                        posts.Select(
                            async post =>
                            {
                                string postId =
                                    post["postId"]
                                        ?.ToString();

                                DocumentSnapshot likeSnapshot =
                                    await _db
                                        .Collection("posts")
                                        .Document(postId)
                                        .Collection("likes")
                                        .Document(
                                            currentUserId)
                                        .GetSnapshotAsync();

                                post["isLiked"] =
                                    likeSnapshot.Exists;

                                return post;
                            });

                return (
                    await Task.WhenAll(
                        likeTasks)
                ).ToList();
            }

            foreach (
                Dictionary<string, object>
                post in posts)
            {
                post["isLiked"] = false;
            }

            return posts;
        }

        // =====================================================
        // DELETE POST
        // =====================================================

        public async Task DeletePost(
            string postId)
        {
            if (string.IsNullOrWhiteSpace(
                    postId))
            {
                throw new Exception(
                    "PostId không hợp lệ.");
            }

            var userDict =
                CurrentUserStore.User
                    as Dictionary<string, object>;

            if (userDict == null)
            {
                throw new Exception(
                    "Người dùng chưa đăng nhập.");
            }

            string currentUserId =
                userDict.ContainsKey("userId")
                    ? Convert.ToString(
                        userDict["userId"])
                    : "";

            if (string.IsNullOrWhiteSpace(
                    currentUserId))
            {
                throw new Exception(
                    "UserId hiện tại không hợp lệ.");
            }

            DocumentReference postReference =
                _db
                    .Collection("posts")
                    .Document(postId);

            DocumentSnapshot postSnapshot =
                await postReference
                    .GetSnapshotAsync();

            if (!postSnapshot.Exists)
            {
                throw new Exception(
                    "Bài viết không tồn tại.");
            }

            Dictionary<string, object>
                postData =
                    postSnapshot
                        .ToDictionary();

            string ownerId =
                postData.ContainsKey("userId")
                    ? Convert.ToString(
                        postData["userId"])
                    : "";

            if (ownerId != currentUserId)
            {
                throw new Exception(
                    "Bạn không có quyền xóa bài viết này.");
            }

            await postReference.DeleteAsync();
        }

        // =====================================================
        // FETCH ORIGINAL SHARED POSTS
        // =====================================================

        public async Task FetchOriginalPostsAsync(
            List<Dictionary<string, object>> posts)
        {
            List<Dictionary<string, object>>
                sharePosts =
                    posts
                        .Where(post =>
                            post.ContainsKey(
                                "postType") &&
                            post["postType"]
                                ?.ToString() ==
                                "share" &&
                            post.ContainsKey(
                                "sharedPostId") &&
                            !string.IsNullOrWhiteSpace(
                                post["sharedPostId"]
                                    ?.ToString()))
                        .ToList();

            if (sharePosts.Count == 0)
            {
                return;
            }

            IEnumerable<Task> fetchTasks =
                sharePosts.Select(
                    async post =>
                    {
                        string sharedPostId =
                            post["sharedPostId"]
                                ?.ToString();

                        try
                        {
                            DocumentSnapshot originalDocument =
                                await _db
                                    .Collection("posts")
                                    .Document(
                                        sharedPostId)
                                    .GetSnapshotAsync();

                            if (!originalDocument.Exists)
                            {
                                return;
                            }

                            Dictionary<string, object>
                                originalData =
                                    originalDocument
                                        .ToDictionary();

                            originalData["postId"] =
                                originalDocument.Id;

                            if (originalData.ContainsKey(
                                    "createdAt") &&
                                originalData["createdAt"]
                                    is Timestamp timestamp)
                            {
                                originalData["createdAt"] =
                                    timestamp
                                        .ToDateTime()
                                        .ToUniversalTime();
                            }

                            post["originalPost"] =
                                originalData;
                        }
                        catch (Exception exception)
                        {
                            System.Diagnostics.Debug.WriteLine(
                                "Không thể tải bài viết gốc: " +
                                exception.Message);
                        }
                    });

            await Task.WhenAll(fetchTasks);
        }

        // =====================================================
        // FOLLOWING IDS
        // =====================================================

        private async Task<HashSet<string>>
            GetFollowingIds(string userId)
        {
            if (string.IsNullOrWhiteSpace(
                    userId))
            {
                return new HashSet<string>();
            }

            QuerySnapshot snapshot =
                await _db
                    .Collection("follows")
                    .WhereEqualTo(
                        "followerId",
                        userId)
                    .GetSnapshotAsync();

            return new HashSet<string>(
                snapshot.Documents
                    .Select(document =>
                        document.GetValue<string>(
                            "followingId")));
        }

        // =====================================================
        // SHARE POST
        // =====================================================

        public async Task<
            Dictionary<string, object>>
            SharePost(
                string currentUserId,
                string originalPostId,
                string content,
                string visibility = "public")
        {
            if (string.IsNullOrWhiteSpace(
                    currentUserId))
            {
                throw new Exception(
                    "Bạn chưa đăng nhập.");
            }

            if (string.IsNullOrWhiteSpace(
                    originalPostId))
            {
                throw new Exception(
                    "Không tìm thấy bài viết cần chia sẻ.");
            }

            if (visibility != "public" &&
                visibility != "followers" &&
                visibility != "private")
            {
                visibility = "public";
            }

            DocumentReference originalPostReference =
                _db
                    .Collection("posts")
                    .Document(originalPostId);

            DocumentSnapshot originalPostDocument =
                await originalPostReference
                    .GetSnapshotAsync();

            if (!originalPostDocument.Exists)
            {
                throw new Exception(
                    "Bài viết gốc không tồn tại hoặc đã bị xóa.");
            }

            Dictionary<string, object>
                originalPost =
                    originalPostDocument
                        .ToDictionary();

            originalPost["postId"] =
                originalPostDocument.Id;

            if (originalPost.ContainsKey(
                    "createdAt") &&
                originalPost["createdAt"]
                    is Timestamp originalTimestamp)
            {
                originalPost["createdAt"] =
                    originalTimestamp
                        .ToDateTime()
                        .ToUniversalTime();
            }

            var userDict =
                CurrentUserStore.User
                    as Dictionary<string, object>;

            if (userDict == null)
            {
                throw new Exception(
                    "Người dùng chưa đăng nhập.");
            }

            string userName =
                userDict.ContainsKey("userName")
                    ? Convert.ToString(
                        userDict["userName"])
                    : "Người dùng";

            string avatar =
                userDict.ContainsKey("avatar")
                    ? Convert.ToString(
                        userDict["avatar"])
                    : "";

            var sharePost =
                new Dictionary<string, object>
                {
                    {
                        "content",
                        content != null
                            ? content.Trim()
                            : ""
                    },
                    {
                        "mediaUrl",
                        null
                    },
                    {
                        "userId",
                        currentUserId
                    },
                    {
                        "userName",
                        userName
                    },
                    {
                        "avatar",
                        avatar
                    },
                    {
                        "visibility",
                        visibility
                    },
                    {
                        "postType",
                        "share"
                    },
                    {
                        "sharedPostId",
                        originalPostId
                    },
                    {
                        "likeCount",
                        0
                    },
                    {
                        "commentCount",
                        0
                    },
                    {
                        "createdAt",
                        Timestamp.GetCurrentTimestamp()
                    }
                };

            DocumentReference documentReference =
                await _db
                    .Collection("posts")
                    .AddAsync(sharePost);

            sharePost["postId"] =
                documentReference.Id;

            sharePost["originalPost"] =
                originalPost;

            return sharePost;
        }

        // =====================================================
        // OFFLINE HELPERS
        // =====================================================

        private string GetCurrentUserId()
        {
            var currentUser =
                CurrentUserStore.User
                    as Dictionary<string, object>;

            if (currentUser == null)
            {
                throw new InvalidOperationException(
                    "Người dùng chưa đăng nhập.");
            }

            if (!currentUser.ContainsKey(
                    "userId") ||
                currentUser["userId"] == null)
            {
                throw new InvalidOperationException(
                    "Người dùng hiện tại không có userId.");
            }

            string userId =
                Convert.ToString(
                    currentUser["userId"]);

            if (string.IsNullOrWhiteSpace(
                    userId))
            {
                throw new InvalidOperationException(
                    "UserId hiện tại không hợp lệ.");
            }

            return userId;
        }

        private void MarkPostsAsRemote(
            List<Dictionary<string, object>> posts)
        {
            if (posts == null)
            {
                return;
            }

            foreach (
                Dictionary<string, object>
                post in posts)
            {
                if (post == null)
                {
                    continue;
                }

                post["dataSource"] =
                    "remote";

                post["isOfflineData"] =
                    false;
            }
        }

        private void TrySaveFeedToCache(
            string currentUserId,
            List<Dictionary<string, object>> posts)
        {
            try
            {
                _postCache.ReplaceFeed(
                    currentUserId,
                    posts);

                int cachedCount =
                    _postCache
                        .GetCachedPostCount(
                            currentUserId);

                System.Diagnostics.Debug.WriteLine(
                    "Số bài đã lưu cache: " +
                    cachedCount);
            }
            catch (Exception cacheException)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Không thể lưu feed vào SQLite: " +
                    cacheException.Message);
            }
        }

        private List<Dictionary<string, object>>
            TryGetCachedFeed(
                string currentUserId)
        {
            try
            {
                List<Dictionary<string, object>>
                    cachedPosts =
                        _postCache
                            .GetCachedFeed(
                                currentUserId);

                System.Diagnostics.Debug.WriteLine(
                    "Số bài đọc từ cache: " +
                    cachedPosts.Count);

                return cachedPosts;
            }
            catch (Exception cacheException)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Không thể đọc feed từ SQLite: " +
                    cacheException.Message);

                return new List<
                    Dictionary<string, object>>();
            }
        }
    }
}
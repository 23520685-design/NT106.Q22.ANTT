using Google.Cloud.Firestore;
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

        public PostService(FirestoreContext context)
        {
            _db = context.Db;
            _storageService = new StorageService();
        }

        public PostService(FirestoreContext context, StorageService storageService)
        {
            _db = context.Db;
            _storageService = storageService;
        }

        public async Task<string> CreatePost(string content, string imagePath, string visibility = "public")
        {
            if (string.IsNullOrWhiteSpace(content) && string.IsNullOrWhiteSpace(imagePath))
                throw new Exception("Bài viết phải có nội dung hoặc hình ảnh.");

            var userDict = CurrentUserStore.User as Dictionary<string, object>;
            if (userDict == null)
                throw new Exception("Người dùng chưa đăng nhập.");

            string userId = userDict.ContainsKey("userId") ? Convert.ToString(userDict["userId"]) : null;
            string userName = userDict.ContainsKey("userName") ? Convert.ToString(userDict["userName"]) : null;
            string avatar = userDict.ContainsKey("avatar") ? Convert.ToString(userDict["avatar"]) : "";

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(userName))
                throw new Exception("Thông tin người dùng không hợp lệ.");

            if (visibility != "public" && visibility != "followers" && visibility != "private")
                visibility = "public";

            string mediaUrl = null;

            if (!string.IsNullOrWhiteSpace(imagePath))
            {
                mediaUrl = await _storageService.UploadImage(imagePath);
            }

            var post = new Dictionary<string, object>
            {
                { "content", content != null ? content.Trim() : "" },
                { "mediaUrl", mediaUrl },
                { "userId", userId },
                { "userName", userName },
                { "avatar", avatar },
                { "visibility", visibility },
                { "likeCount", 0 },
                { "commentCount", 0 },
                { "createdAt", Timestamp.GetCurrentTimestamp() }
            };

            DocumentReference docRef = await _db.Collection("posts").AddAsync(post);
            return docRef.Id;
        }

        public async Task<List<Dictionary<string, object>>> GetFeed()
        {
            var snapshot = await _db.Collection("posts")
                .WhereEqualTo("visibility", "public")
                .OrderByDescending("createdAt")
                .Limit(50)
                .GetSnapshotAsync();

            var userDict = CurrentUserStore.User as Dictionary<string, object>;
            string currentUserId = userDict != null && userDict.ContainsKey("userId")
                ? userDict["userId"]?.ToString()
                : "";

            var allPosts = snapshot.Documents
                .Select(doc =>
                {
                    var data = doc.ToDictionary();
                    data["postId"] = doc.Id;

                    if (data.ContainsKey("createdAt") && data["createdAt"] is Timestamp ts)
                    {
                        data["createdAt"] = ts.ToDateTime().ToUniversalTime();
                    }

                    return data;
                })
                .ToList();

            // Ưu tiên bài mới nhất
            var newest = allPosts.Take(5).ToList();

            // Các bài còn lại random theo từng user
            int seed = !string.IsNullOrEmpty(currentUserId)
                ? currentUserId.GetHashCode()
                : Environment.TickCount;

            Random rng = new Random(seed);

            var randomizedOlderPosts = allPosts
                .Skip(5)
                .OrderBy(x => rng.Next())
                .Take(15)
                .ToList();

            var posts = newest.Concat(randomizedOlderPosts).ToList();

            if (!string.IsNullOrEmpty(currentUserId))
            {
                var likeTasks = posts.Select(async post =>
                {
                    string postId = post["postId"]?.ToString();
                    var likeSnap = await _db.Collection("posts")
                        .Document(postId)
                        .Collection("likes")
                        .Document(currentUserId)
                        .GetSnapshotAsync();

                    post["isLiked"] = likeSnap.Exists;
                    return post;
                });

                return (await Task.WhenAll(likeTasks)).ToList();
            }

            foreach (var post in posts)
                post["isLiked"] = false;

            return posts;
        }

        // Lấy tất cả bài viết của một user cụ thể
        public async Task<List<Dictionary<string, object>>> GetUserPosts(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return new List<Dictionary<string, object>>();

            var snapshot = await _db.Collection("posts")
                .WhereEqualTo("userId", userId)
                .OrderByDescending("createdAt")
                .Limit(50)
                .GetSnapshotAsync();

            var currentUserDict = CurrentUserStore.User as Dictionary<string, object>;
            string currentUserId = currentUserDict != null && currentUserDict.ContainsKey("userId")
                ? currentUserDict["userId"]?.ToString()
                : "";

            var posts = snapshot.Documents.Select(doc =>
            {
                var data = doc.ToDictionary();
                data["postId"] = doc.Id;

                if (data.ContainsKey("createdAt") && data["createdAt"] is Timestamp ts)
                {
                    data["createdAt"] = ts.ToDateTime().ToUniversalTime();
                }

                return data;
            }).ToList();

            // Kiểm tra isLiked cho current user
            if (!string.IsNullOrEmpty(currentUserId))
            {
                var likeTasks = posts.Select(async post =>
                {
                    string postId = post["postId"]?.ToString();
                    var likeSnap = await _db.Collection("posts")
                        .Document(postId)
                        .Collection("likes")
                        .Document(currentUserId)
                        .GetSnapshotAsync();

                    post["isLiked"] = likeSnap.Exists;
                    return post;
                });

                return (await Task.WhenAll(likeTasks)).ToList();
            }

            foreach (var post in posts)
                post["isLiked"] = false;

            return posts;
        }

        public async Task DeletePost(string postId)
        {
            if (string.IsNullOrWhiteSpace(postId))
                throw new Exception("PostId không hợp lệ.");

            var userDict = CurrentUserStore.User as Dictionary<string, object>;

            if (userDict == null)
                throw new Exception("Người dùng chưa đăng nhập.");

            string currentUserId = userDict.ContainsKey("userId")
                ? Convert.ToString(userDict["userId"])
                : "";

            if (string.IsNullOrWhiteSpace(currentUserId))
                throw new Exception("UserId hiện tại không hợp lệ.");

            var postRef = _db.Collection("posts").Document(postId);
            var postSnap = await postRef.GetSnapshotAsync();

            if (!postSnap.Exists)
                throw new Exception("Bài viết không tồn tại.");

            var postData = postSnap.ToDictionary();

            string ownerId = postData.ContainsKey("userId")
                ? Convert.ToString(postData["userId"])
                : "";

            if (ownerId != currentUserId)
                throw new Exception("Bạn không có quyền xóa bài viết này.");

            await postRef.DeleteAsync();
        }

        private async Task<HashSet<string>> GetFollowingIds(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return new HashSet<string>();

            var snapshot = await _db.Collection("follows")
                .WhereEqualTo("followerId", userId)
                .GetSnapshotAsync();

            return new HashSet<string>(
                snapshot.Documents.Select(d => d.GetValue<string>("followingId"))
            );
        }
    }
}

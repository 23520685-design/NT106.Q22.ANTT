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

        // Replace this block in GetFeed():
        if (data.ContainsKey("createdAt") && data["createdAt"] is Timestamp ts)
        {
            // data["createdAt"] = ts.Seconds; // <-- Remove this line
            data["createdAt"] = ts.ToDateTime().ToUniversalTime(); // Store as UTC DateTime
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

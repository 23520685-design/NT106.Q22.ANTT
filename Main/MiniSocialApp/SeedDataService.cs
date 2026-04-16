using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniSocialApp.Services
{
    public class SeedDataService
    {
        private readonly FirestoreDb _db;

        public SeedDataService(FirestoreContext context)
        {
            _db = context.Db;
        }

        public async Task SeedStarterPostsForUser(Dictionary<string, object> user)
        {
            if (user == null) return;

            string userId = user.ContainsKey("userId") ? user["userId"]?.ToString() : "";
            string userName = user.ContainsKey("userName") ? user["userName"]?.ToString() : "User";
            string avatar = user.ContainsKey("avatar") ? user["avatar"]?.ToString() : "";

            if (string.IsNullOrEmpty(userId)) return;

            var sampleContents = new List<string>
            {
                "Chào mọi người, mình mới tham gia app 👋",
                "Hôm nay là một ngày khá ổn.",
                "Một chút tích cực cho newsfeed ✨",
                "Đăng bài đầu tiên để làm quen với mọi người.",
                "Ai cũng cần một khởi đầu mới.",
                "Mỗi ngày cố gắng thêm một chút.",
                "Vừa vào app nên đăng bài test thử 😄",
                "Hy vọng sẽ kết nối được với nhiều người thú vị.",
                "Một ngày mới, một năng lượng mới.",
                "Không cần quá nhanh, chỉ cần tiến lên."
            };

            var rng = new Random(userId.GetHashCode());
            int seedCount = rng.Next(4, 7); // 4-6 bài

            var pickedContents = sampleContents
                .OrderBy(_ => rng.Next())
                .Take(seedCount)
                .ToList();

            var batch = _db.StartBatch();
            int index = 0;

            foreach (var content in pickedContents)
            {
                var postRef = _db.Collection("posts").Document();

                int minutesAgo = rng.Next(10, 60 * 24 * 3) + index * 5;

                var post = new Dictionary<string, object>
                {
                    { "content", content },
                    { "mediaUrl", null },
                    { "userId", userId },
                    { "userName", userName },
                    { "avatar", avatar },
                    { "visibility", "public" },
                    { "likeCount", rng.Next(0, 20) },
                    { "commentCount", rng.Next(0, 5) },
                    { "createdAt", Timestamp.FromDateTime(DateTime.UtcNow.AddMinutes(-minutesAgo)) }
                };

                batch.Set(postRef, post);
                index++;
            }

            batch.Update(_db.Collection("users").Document(userId), new Dictionary<string, object>
            {
                { "postCount", seedCount }
            });

            await batch.CommitAsync();
        }
    }
}
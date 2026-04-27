using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSocialApp.Services
{
    public class UserService
    {
        private readonly FirestoreDb _db;
        public UserService(FirestoreContext context)
        {
            _db = context.Db;
        }

        public async Task<Dictionary<string, object>> LoginOrCreate(string userName, string phone)
        {

        
            // tìm user theo phone
            var query = _db.Collection("users")
                          .WhereEqualTo("phone", phone);

            var snapshot = await query.GetSnapshotAsync();

            if (snapshot.Count > 0)
            {
                return snapshot.Documents[0].ToDictionary();
            }

            // tạo mới
            var userId = Guid.NewGuid().ToString();

            var user = new Dictionary<string, object>
    {
        { "userId", userId },
        { "userName", userName },
        { "avatar", "https://i.pravatar.cc/150?u=" + userId },
        { "phone", phone },
        { "followersCount", 0 },
        { "followingCount", 0 },
        { "postCount", 0 },
        { "createdAt", Timestamp.GetCurrentTimestamp() }
    };

            await _db.Collection("users").Document(userId).SetAsync(user);

            return user;
        }

        // Tìm kiếm user theo tên hoặc số điện thoại, loại trừ user hiện tại
        public async Task<List<Dictionary<string, object>>> SearchUsers(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<Dictionary<string, object>>();

            keyword = keyword.Trim().ToLower();

            var currentUserDict = CurrentUserStore.User as Dictionary<string, object>;
            string currentUserId = currentUserDict != null && currentUserDict.ContainsKey("userId")
                ? currentUserDict["userId"]?.ToString()
                : "";

            var snapshot = await _db.Collection("users")
                .Limit(100)
                .GetSnapshotAsync();

            var users = snapshot.Documents
                .Select(doc =>
                {
                    var data = doc.ToDictionary();

                    if (!data.ContainsKey("userId"))
                        data["userId"] = doc.Id;

                    return data;
                })
                .Where(user =>
                {
                    string userId = user.ContainsKey("userId") ? user["userId"]?.ToString() : "";
                    string userName = user.ContainsKey("userName") ? user["userName"]?.ToString() : "";
                    string phone = user.ContainsKey("phone") ? user["phone"]?.ToString() : "";

                    if (!string.IsNullOrEmpty(currentUserId) && userId == currentUserId)
                        return false;

                    return userName.ToLower().Contains(keyword)
                        || phone.ToLower().Contains(keyword);
                })
                .Take(10)
                .ToList();

            return users;
        }
    }
}

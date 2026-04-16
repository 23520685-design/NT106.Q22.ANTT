using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniSocialApp.Services
{
    internal class UserService
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
    }
}

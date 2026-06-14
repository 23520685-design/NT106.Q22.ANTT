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
    // Đăng nhập hoặc tạo mới user dựa trên số điện thoại
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
        { "bio", "" },
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
    // Cập nhật profile của user hiện tại
        public async Task<Dictionary<string, object>> UpdateUserProfile(
            string userName,
            string bio,
            string avatar
        )
        {
            var userDict = CurrentUserStore.User as Dictionary<string, object>;
            if (userDict == null)
                throw new Exception("Người dùng chưa đăng nhập.");

            string userId = userDict.ContainsKey("userId")
                ? Convert.ToString(userDict["userId"])
                : null;

            if (string.IsNullOrWhiteSpace(userId))
                throw new Exception("UserId không hợp lệ.");

            if (string.IsNullOrWhiteSpace(userName))
                throw new Exception("Tên người dùng không được để trống.");

            userName = userName.Trim();
            bio = bio != null ? bio.Trim() : "";
            avatar = avatar != null ? avatar.Trim() : "";

            var updateData = new Dictionary<string, object>
            {
                { "userName", userName },
                { "bio", bio },
                { "updatedAt", Timestamp.GetCurrentTimestamp() }
            };

            if (!string.IsNullOrWhiteSpace(avatar))
            {
                updateData["avatar"] = avatar;
            }

            var userRef = _db.Collection("users").Document(userId);
            await userRef.UpdateAsync(updateData);

            var updatedSnap = await userRef.GetSnapshotAsync();
            if (!updatedSnap.Exists)
                throw new Exception("Không tìm thấy người dùng.");

            var updatedUser = updatedSnap.ToDictionary();

            if (!updatedUser.ContainsKey("userId"))
                updatedUser["userId"] = userId;

            CurrentUserStore.User = updatedUser;

            return updatedUser;
        }

     // Lấy thông tin profile của user, bao gồm bài viết và thống kê
        public async Task<Dictionary<string, object>> GetUserProfile(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new Exception("UserId không hợp lệ.");

            var userDoc = await _db.Collection("users").Document(userId).GetSnapshotAsync();

            if (!userDoc.Exists)
                throw new Exception("Không tìm thấy người dùng.");

            var user = userDoc.ToDictionary();

            if (!user.ContainsKey("userId"))
                user["userId"] = userDoc.Id;

            // Lấy bài viết của user
            var postSnapshot = await _db.Collection("posts")
                .WhereEqualTo("userId", userId)
                .OrderByDescending("createdAt")
                .Limit(50)
                .GetSnapshotAsync();

            var posts = postSnapshot.Documents
                .Select(doc =>
                {
                    var data = doc.ToDictionary();

                    if (!data.ContainsKey("postId"))
                        data["postId"] = doc.Id;

                    return data;
                })
                .ToList();

            int totalLikes = posts.Sum(p =>
                p.ContainsKey("likeCount") ? Convert.ToInt32(p["likeCount"]) : 0
            );

            int totalComments = posts.Sum(p =>
                p.ContainsKey("commentCount") ? Convert.ToInt32(p["commentCount"]) : 0
            );

            user["posts"] = posts;
            user["stats"] = new Dictionary<string, object>
    {
        { "posts", posts.Count },
        { "likes", totalLikes },
        { "comments", totalComments },
        { "friends", user.ContainsKey("followersCount") ? user["followersCount"] : 0 }
    };

            return user;
        }
    }
}

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

            var currentUserDict = CurrentUserStore.User as Dictionary<string, object>;

            string currentUserId = currentUserDict != null && currentUserDict.ContainsKey("userId")
                ? Convert.ToString(currentUserDict["userId"])
                : "";

            bool isFollowing = false;

            if (!string.IsNullOrWhiteSpace(currentUserId) && currentUserId != userId)
            {
                string followId = currentUserId + "_" + userId;

                var followDoc = await _db.Collection("follows")
                    .Document(followId)
                    .GetSnapshotAsync();

                isFollowing = followDoc.Exists;
            }

            int followersCount = user.ContainsKey("followersCount")
                ? Convert.ToInt32(user["followersCount"])
                : 0;

            int followingCount = user.ContainsKey("followingCount")
                ? Convert.ToInt32(user["followingCount"])
                : 0;

            user["isFollowing"] = isFollowing;
            user["followersCount"] = followersCount;
            user["followingCount"] = followingCount;

            user["posts"] = posts;

            user["stats"] = new Dictionary<string, object>
{
    { "posts", posts.Count },
    { "followers", followersCount },
    { "following", followingCount },
    { "likes", totalLikes },
    { "comments", totalComments }
};

            return user;
        }

        public async Task<Dictionary<string, object>> ToggleFollow(string targetUserId)
        {
            var currentUserDict = CurrentUserStore.User as Dictionary<string, object>;

            if (currentUserDict == null)
                throw new Exception("Người dùng chưa đăng nhập.");

            string currentUserId = currentUserDict.ContainsKey("userId")
                ? Convert.ToString(currentUserDict["userId"])
                : "";

            if (string.IsNullOrWhiteSpace(currentUserId))
                throw new Exception("UserId hiện tại không hợp lệ.");

            if (string.IsNullOrWhiteSpace(targetUserId))
                throw new Exception("UserId cần follow không hợp lệ.");

            if (currentUserId == targetUserId)
                throw new Exception("Bạn không thể follow chính mình.");

            var currentUserRef = _db.Collection("users").Document(currentUserId);
            var targetUserRef = _db.Collection("users").Document(targetUserId);

            string followId = currentUserId + "_" + targetUserId;
            var followRef = _db.Collection("follows").Document(followId);

            bool isFollowing = false;
            int followersCount = 0;
            int followingCount = 0;

            await _db.RunTransactionAsync(async transaction =>
            {
                var followSnap = await transaction.GetSnapshotAsync(followRef);
                var currentUserSnap = await transaction.GetSnapshotAsync(currentUserRef);
                var targetUserSnap = await transaction.GetSnapshotAsync(targetUserRef);

                if (!currentUserSnap.Exists)
                    throw new Exception("Không tìm thấy người dùng hiện tại.");

                if (!targetUserSnap.Exists)
                    throw new Exception("Không tìm thấy người dùng cần follow.");

                int targetFollowers = 0;
                int currentFollowing = 0;

                var targetData = targetUserSnap.ToDictionary();
                var currentData = currentUserSnap.ToDictionary();

                if (targetData.ContainsKey("followersCount"))
                    targetFollowers = Convert.ToInt32(targetData["followersCount"]);

                if (currentData.ContainsKey("followingCount"))
                    currentFollowing = Convert.ToInt32(currentData["followingCount"]);

                if (followSnap.Exists)
                {
                    transaction.Delete(followRef);

                    targetFollowers = Math.Max(0, targetFollowers - 1);
                    currentFollowing = Math.Max(0, currentFollowing - 1);

                    transaction.Update(targetUserRef, new Dictionary<string, object>
            {
                { "followersCount", targetFollowers }
            });

                    transaction.Update(currentUserRef, new Dictionary<string, object>
            {
                { "followingCount", currentFollowing }
            });

                    isFollowing = false;
                }
                else
                {
                    transaction.Set(followRef, new Dictionary<string, object>
            {
                { "followerId", currentUserId },
                { "followingId", targetUserId },
                { "createdAt", Timestamp.GetCurrentTimestamp() }
            });

                    targetFollowers++;
                    currentFollowing++;

                    transaction.Update(targetUserRef, new Dictionary<string, object>
            {
                { "followersCount", targetFollowers }
            });

                    transaction.Update(currentUserRef, new Dictionary<string, object>
            {
                { "followingCount", currentFollowing }
            });

                    isFollowing = true;
                }

                followersCount = targetFollowers;
                followingCount = currentFollowing;
            });

            return new Dictionary<string, object>
    {
        { "targetUserId", targetUserId },
        { "isFollowing", isFollowing },
        { "followersCount", followersCount },
        { "followingCount", followingCount }
    };
        }

        // Lấy danh sách người dùng mà user hiện tại đang follow
        public async Task<List<Dictionary<string, object>>> GetFollowers(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new Exception("UserId không hợp lệ.");

            var followSnapshot = await _db.Collection("follows")
                .WhereEqualTo("followingId", userId)
                .OrderByDescending("createdAt")
                .Limit(50)
                .GetSnapshotAsync();

            var followerIds = followSnapshot.Documents
                .Select(doc =>
                {
                    var data = doc.ToDictionary();
                    return data.ContainsKey("followerId")
                        ? Convert.ToString(data["followerId"])
                        : "";
                })
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var users = new List<Dictionary<string, object>>();

            foreach (var followerId in followerIds)
            {
                var userDoc = await _db.Collection("users")
                    .Document(followerId)
                    .GetSnapshotAsync();

                if (!userDoc.Exists)
                    continue;

                var user = userDoc.ToDictionary();

                if (!user.ContainsKey("userId"))
                    user["userId"] = userDoc.Id;

                users.Add(new Dictionary<string, object>
        {
            { "userId", user.ContainsKey("userId") ? user["userId"] : userDoc.Id },
            { "userName", user.ContainsKey("userName") ? user["userName"] : "Người dùng" },
            { "avatar", user.ContainsKey("avatar") ? user["avatar"] : "" },
            { "bio", user.ContainsKey("bio") ? user["bio"] : "" },
            { "phone", user.ContainsKey("phone") ? user["phone"] : "" }
        });
            }

            return users;
        }

        // Lấy danh sách người dùng mà user hiện tại đang follow
        public async Task<List<Dictionary<string, object>>> GetFollowing(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new Exception("UserId không hợp lệ.");

            var followSnapshot = await _db.Collection("follows")
                .WhereEqualTo("followerId", userId)
                .OrderByDescending("createdAt")
                .Limit(50)
                .GetSnapshotAsync();

            var followingIds = followSnapshot.Documents
                .Select(doc =>
                {
                    var data = doc.ToDictionary();
                    return data.ContainsKey("followingId")
                        ? Convert.ToString(data["followingId"])
                        : "";
                })
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var users = new List<Dictionary<string, object>>();

            foreach (var followingId in followingIds)
            {
                var userDoc = await _db.Collection("users")
                    .Document(followingId)
                    .GetSnapshotAsync();

                if (!userDoc.Exists)
                    continue;

                var user = userDoc.ToDictionary();

                if (!user.ContainsKey("userId"))
                    user["userId"] = userDoc.Id;

                users.Add(new Dictionary<string, object>
        {
            { "userId", user.ContainsKey("userId") ? user["userId"] : userDoc.Id },
            { "userName", user.ContainsKey("userName") ? user["userName"] : "Người dùng" },
            { "avatar", user.ContainsKey("avatar") ? user["avatar"] : "" },
            { "bio", user.ContainsKey("bio") ? user["bio"] : "" },
            { "phone", user.ContainsKey("phone") ? user["phone"] : "" }
        });
            }

            return users;
        }
    }
}

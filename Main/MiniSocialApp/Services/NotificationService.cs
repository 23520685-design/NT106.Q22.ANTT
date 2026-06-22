using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniSocialApp.Services
{
    public class NotificationService
    {
        private readonly FirestoreDb _db;

        public NotificationService(FirestoreContext context)
        {
            _db = context.Db;
        }

        public async Task CreateNotification(
            string receiverUserId,
            string senderUserId,
            string type,
            string message,
            string postId = "",
            string targetUserId = "")
        {
            if (string.IsNullOrWhiteSpace(receiverUserId))
                return;

            if (string.IsNullOrWhiteSpace(senderUserId))
                return;

            // Không tự gửi notification cho chính mình
            if (receiverUserId == senderUserId)
                return;

            var senderDoc = await _db.Collection("users")
                .Document(senderUserId)
                .GetSnapshotAsync();

            string senderName = "Người dùng";
            string senderAvatar = "";

            if (senderDoc.Exists)
            {
                var sender = senderDoc.ToDictionary();

                senderName = sender.ContainsKey("userName")
                    ? Convert.ToString(sender["userName"])
                    : "Người dùng";

                senderAvatar = sender.ContainsKey("avatar")
                    ? Convert.ToString(sender["avatar"])
                    : "";
            }

            var notification = new Dictionary<string, object>
            {
                { "receiverUserId", receiverUserId },
                { "senderUserId", senderUserId },
                { "senderName", senderName },
                { "senderAvatar", senderAvatar },
                { "type", type },
                { "message", message },
                { "postId", postId ?? "" },
                { "targetUserId", targetUserId ?? "" },
                { "isRead", false },
                { "createdAt", Timestamp.GetCurrentTimestamp() }
            };

            await _db.Collection("notifications").AddAsync(notification);
        }

        public async Task<Dictionary<string, object>> GetNotifications(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new Exception("UserId không hợp lệ.");

            var snapshot = await _db.Collection("notifications")
                .WhereEqualTo("receiverUserId", userId)
                .Limit(50)
                .GetSnapshotAsync();

            var notifications = snapshot.Documents
                .Select(doc =>
                {
                    var data = doc.ToDictionary();

                    data["notificationId"] = doc.Id;

                    if (data.ContainsKey("createdAt") && data["createdAt"] is Timestamp ts)
                    {
                        data["createdAt"] = ts.ToDateTime().ToUniversalTime();
                    }

                    return data;
                })
                .OrderByDescending(n =>
                {
                    if (n.ContainsKey("createdAt") && n["createdAt"] is DateTime dt)
                        return dt;

                    return DateTime.MinValue;
                })
                .ToList();

            int unreadCount = notifications.Count(n =>
                n.ContainsKey("isRead") &&
                n["isRead"] is bool isRead &&
                isRead == false
            );

            return new Dictionary<string, object>
            {
                { "notifications", notifications },
                { "unreadCount", unreadCount }
            };
        }

        public async Task MarkAsRead(string notificationId)
        {
            if (string.IsNullOrWhiteSpace(notificationId))
                throw new Exception("NotificationId không hợp lệ.");

            await _db.Collection("notifications")
                .Document(notificationId)
                .UpdateAsync(new Dictionary<string, object>
                {
                    { "isRead", true }
                });
        }

        public async Task MarkAllAsRead(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new Exception("UserId không hợp lệ.");

            var snapshot = await _db.Collection("notifications")
                .WhereEqualTo("receiverUserId", userId)
                .WhereEqualTo("isRead", false)
                .GetSnapshotAsync();

            WriteBatch batch = _db.StartBatch();

            foreach (var doc in snapshot.Documents)
            {
                batch.Update(doc.Reference, new Dictionary<string, object>
                {
                    { "isRead", true }
                });
            }

            await batch.CommitAsync();
        }
    }
}
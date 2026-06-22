using MiniSocialApp.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiniSocialApp.Controllers
{
    public class NotificationController
    {
        private readonly NotificationService _notificationService;

        public NotificationController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        private string GetCurrentUserId()
        {
            var userDict = MiniSocialApp.CurrentUserStore.User
                as Dictionary<string, object>;

            if (userDict != null && userDict.ContainsKey("userId"))
            {
                return userDict["userId"]?.ToString();
            }

            return "";
        }

        public async Task<object> GetNotifications()
        {
            string currentUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(currentUserId))
                throw new Exception("Bạn chưa đăng nhập.");

            var data = await _notificationService.GetNotifications(currentUserId);

            return new
            {
                type = "NOTIFICATIONS_DATA",
                data = data
            };
        }

        public async Task<object> MarkNotificationRead(dynamic data)
        {
            string notificationId = data.notificationId != null
                ? (string)data.notificationId
                : "";

            await _notificationService.MarkAsRead(notificationId);

            return new
            {
                type = "MARK_NOTIFICATION_READ_SUCCESS",
                data = new
                {
                    notificationId = notificationId
                }
            };
        }

        public async Task<object> MarkAllNotificationsRead()
        {
            string currentUserId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(currentUserId))
                throw new Exception("Bạn chưa đăng nhập.");

            await _notificationService.MarkAllAsRead(currentUserId);

            return new
            {
                type = "MARK_ALL_NOTIFICATIONS_READ_SUCCESS"
            };
        }
    }
}
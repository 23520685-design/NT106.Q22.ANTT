using MiniSocialApp.Services;
using System.Threading.Tasks;

namespace MiniSocialApp.Controllers
{
    public class UserController
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        public async Task<object> SearchUsers(dynamic data)
        {
            string keyword = data.keyword != null ? (string)data.keyword : "";

            var users = await _userService.SearchUsers(keyword);

            return new
            {
                type = "SEARCH_USER_RESULT",
                data = users
            };
        }

        public async Task<object> UpdateProfile(dynamic data)
        {
            string userName = data.userName != null ? (string)data.userName : "";
            string bio = data.bio != null ? (string)data.bio : "";
            string avatar = data.avatar != null ? (string)data.avatar : "";

            var updatedUser = await _userService.UpdateUserProfile(userName, bio, avatar);

            return new
            {
                type = "UPDATE_PROFILE_SUCCESS",
                data = updatedUser
            };
        }

        public async Task<object> ToggleFollow(dynamic data)
        {
            string targetUserId = data.targetUserId != null
                ? (string)data.targetUserId
                : "";

            var result = await _userService.ToggleFollow(targetUserId);

            return new
            {
                type = "FOLLOW_UPDATED",
                data = result
            };
        }

        public async Task<object> GetUserProfile(dynamic data)
        {
            string userId = data.userId != null ? (string)data.userId : "";

            var profile = await _userService.GetUserProfile(userId);

            return new
            {
                type = "PROFILE_DATA",
                data = profile
            };
        }

        public async Task<object> OpenUserProfile(dynamic data)
        {
            string userId = data.userId != null ? (string)data.userId : "";

            var profile = await _userService.GetUserProfile(userId);

            return new
            {
                type = "OPEN_PROFILE_PAGE",
                data = profile
            };
        }

        public async Task<object> GetFollowers(dynamic data)
        {
            string userId = data.userId != null ? (string)data.userId : "";

            var followers = await _userService.GetFollowers(userId);

            return new
            {
                type = "FOLLOWERS_DATA",
                data = followers
            };
        }

        public async Task<object> GetFollowing(dynamic data)
        {
            string userId = data.userId != null ? (string)data.userId : "";

            var following = await _userService.GetFollowing(userId);

            return new
            {
                type = "FOLLOWING_DATA",
                data = following
            };
        }

    }
}
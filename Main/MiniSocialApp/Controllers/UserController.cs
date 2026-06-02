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

    }
}
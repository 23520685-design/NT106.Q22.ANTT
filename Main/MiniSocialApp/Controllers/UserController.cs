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
    }
}
using System.Threading.Tasks;
using MiniSocialApp.Services;

namespace MiniSocialApp.Controllers
{
    // ✅ FIX 7: Chuyển sang instance class, inject LikeService → dùng chung FirestoreContext
    public class LikeController
    {
        private readonly LikeService _likeService;

        public LikeController(LikeService likeService)
        {
            _likeService = likeService;
        }

        public async Task<object> ToggleLike(dynamic data)
        {
            string postId = data.postId;
            var result = await _likeService.ToggleLike(postId);

            return new
            {
                type = "LIKE_UPDATED",
                data = result
            };
        }
    }
}

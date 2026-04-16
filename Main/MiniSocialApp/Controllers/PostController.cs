using MiniSocialApp.Services;
using System.Threading.Tasks;

namespace MiniSocialApp.Controllers
{
    public class PostController
    {
        private readonly PostService _postService;

        public PostController(PostService postService)
        {
            _postService = postService;
        }

        public async Task<object> CreatePost(dynamic data)
        {
            string content    = (string)data.content;
            string imagePath  = data.imagePath  != null ? (string)data.imagePath  : null;
            string visibility = data.visibility != null ? (string)data.visibility : "public";

            var postId = await _postService.CreatePost(content, imagePath, visibility);

            return new
            {
                type = "CREATE_POST_SUCCESS",
                data = new { postId }
            };
        }

        public async Task<object> GetFeed()
        {
            var posts = await _postService.GetFeed();

            return new
            {
                type = "FEED_DATA",
                data = posts
            };
        }
    }
}

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

        // Lấy bài viết của user (dùng cho Profile)
        public async Task<object> GetUserPosts(dynamic data)
        {
            string userId = null;

            // Nếu có truyền userId thì dùng, không thì lấy current user
            if (data != null && data.userId != null)
            {
                userId = (string)data.userId;
            }
            else
            {
                var userDict = MiniSocialApp.CurrentUserStore.User as System.Collections.Generic.Dictionary<string, object>;
                if (userDict != null && userDict.ContainsKey("userId"))
                    userId = userDict["userId"]?.ToString();
            }

            var posts = await _postService.GetUserPosts(userId);

            return new
            {
                type = "PROFILE_POSTS_DATA",
                data = posts
            };
        }

        public async Task<object> DeletePost(dynamic data)
        {
            string postId = data.postId != null
                ? (string)data.postId
                : "";

            await _postService.DeletePost(postId);

            return new
            {
                type = "DELETE_POST_SUCCESS",
                data = new
                {
                    postId = postId
                }
            };
        }

        public async Task<object> SharePost(dynamic data)
        {
            string postId = data.postId != null ? (string)data.postId : "";
            string content = data.content != null ? (string)data.content : "";
            string visibility = data.visibility != null ? (string)data.visibility : "public";

            string currentUserId = "";

            var userDict = MiniSocialApp.CurrentUserStore.User
                as System.Collections.Generic.Dictionary<string, object>;

            if (userDict != null && userDict.ContainsKey("userId"))
            {
                currentUserId = userDict["userId"]?.ToString();
            }

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                throw new System.Exception("Bạn chưa đăng nhập.");
            }

            var sharedPost = await _postService.SharePost(
                currentUserId,
                postId,
                content,
                visibility
            );

            return new
            {
                type = "SHARE_POST_SUCCESS",
                data = sharedPost
            };
        }
    }
}

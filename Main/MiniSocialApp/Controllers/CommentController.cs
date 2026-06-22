using MiniSocialApp.Services;
using System.Threading.Tasks;

namespace MiniSocialApp.Controllers
{
    public class CommentController
    {
        private readonly CommentService _commentService;

        public CommentController(CommentService commentService)
        {
            _commentService = commentService;
        }

        public async Task<object> CreateComment(dynamic data)
        {
            string postId = data.postId != null ? (string)data.postId : "";
            string content = data.content != null ? (string)data.content : "";

            var comment = await _commentService.CreateComment(postId, content);

            return new
            {
                type = "CREATE_COMMENT_SUCCESS",
                data = comment
            };
        }

        public async Task<object> GetComments(dynamic data)
        {
            string postId = data.postId != null ? (string)data.postId : "";
            var comments = await _commentService.GetComments(postId);

            return new
            {
                type = "COMMENTS_DATA",
                data = new
                {
                    postId = postId,
                    comments = comments
                }
            };
        }
    }
}

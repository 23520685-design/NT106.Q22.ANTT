using MiniSocialApp.Controllers;
using Newtonsoft.Json;
using System.Threading.Tasks;

public class MessageHandler
{
    private readonly PostController _postController;
    private readonly LikeController _likeController;
    private readonly UserController _userController;
    private readonly CommentController _commentController;

    public MessageHandler(
    PostController postController,
    LikeController likeController,
    UserController userController,
    CommentController commentController)
    {
        _postController = postController;
        _likeController = likeController;
        _userController = userController;
        _commentController = commentController;
    }

    public async Task<string> Handle(string json)
    {
        try
        {
            dynamic msg = JsonConvert.DeserializeObject(json);
            string type = msg.type;

            switch (type)
            {
                case "CREATE_POST":
                    return JsonConvert.SerializeObject(
                        await _postController.CreatePost(msg.data)
                    );

                case "DELETE_POST":
                    return JsonConvert.SerializeObject(
                        await _postController.DeletePost(msg.data)
                    );

                case "GET_FEED":
                    return JsonConvert.SerializeObject(
                        await _postController.GetFeed()
                    );

                case "GET_PROFILE_POSTS":
                    return JsonConvert.SerializeObject(
                        await _postController.GetUserPosts(msg.data)
                    );

                case "TOGGLE_LIKE":
                    return JsonConvert.SerializeObject(
                        await _likeController.ToggleLike(msg.data)
                    );

                case "SEARCH_USER":
                    return JsonConvert.SerializeObject(
                        await _userController.SearchUsers(msg.data)
                    );


                case "OPEN_USER_PROFILE":
                    return JsonConvert.SerializeObject(
                        await _userController.OpenUserProfile(msg.data)
                    );

                case "UPDATE_PROFILE":
                    return JsonConvert.SerializeObject(
                        await _userController.UpdateProfile(msg.data)
                    );

                case "GET_COMMENTS":
                    return JsonConvert.SerializeObject(
                        await _commentController.GetComments(msg.data)
                    );

                case "CREATE_COMMENT":
                    return JsonConvert.SerializeObject(
                        await _commentController.CreateComment(msg.data)
                    );

                case "GET_USER_PROFILE":
                    return JsonConvert.SerializeObject(
                        await _userController.GetUserProfile(msg.data)
                    );

                case "TOGGLE_FOLLOW":
                    return JsonConvert.SerializeObject(
                        await _userController.ToggleFollow(msg.data)
                    );

                case "GET_FOLLOWERS":
                    return JsonConvert.SerializeObject(
                        await _userController.GetFollowers(msg.data)
                    );

                case "GET_FOLLOWING":
                    return JsonConvert.SerializeObject(
                        await _userController.GetFollowing(msg.data)
                    );

                default:
                    return JsonConvert.SerializeObject(new
                    {
                        type = "ERROR",
                        message = $"Unknown message type: {type}"
                    });
            }
        }
        catch (System.Exception ex)
        {
            return JsonConvert.SerializeObject(new
            {
                type = "ERROR",
                message = ex.Message
            });
        }
    }
}

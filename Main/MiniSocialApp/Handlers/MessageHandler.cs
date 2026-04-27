using MiniSocialApp.Controllers;
using Newtonsoft.Json;
using System.Threading.Tasks;

public class MessageHandler
{
    private readonly PostController _postController;
    private readonly LikeController _likeController;
    private readonly UserController _userController;

    public MessageHandler(
    PostController postController,
    LikeController likeController,
    UserController userController)
    {
        _postController = postController;
        _likeController = likeController;
        _userController = userController;
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

                case "GET_FEED":
                    return JsonConvert.SerializeObject(
                        await _postController.GetFeed()
                    );

                case "TOGGLE_LIKE":
                    return JsonConvert.SerializeObject(
                        await _likeController.ToggleLike(msg.data)
                    );

                case "SEARCH_USER":
                    return JsonConvert.SerializeObject(
                        await _userController.SearchUsers(msg.data)
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

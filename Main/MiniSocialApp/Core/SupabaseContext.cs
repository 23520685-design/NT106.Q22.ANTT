using MiniSocialApp.Config;
using System.Net.Http;

public class SupabaseContext
{
    public HttpClient Client { get; private set; }

    public SupabaseContext()
    {
        Client = new HttpClient();
        Client.DefaultRequestHeaders.Add("apikey", SupabaseConfig.ApiKey);
        Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {SupabaseConfig.ApiKey}");
    }
}

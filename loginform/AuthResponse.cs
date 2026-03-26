using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace loginform
{
    public class AuthResponse
    {
        [JsonProperty("access_token")]
        public string Access_token { get; set; }

        public static AuthResponse Exchange(string authCode, string clientid, string secret, string redirectURI)
        {
            var request = (HttpWebRequest)WebRequest.Create("https://accounts.google.com/o/oauth2/token");
            string postData = string.Format("code={0}&client_id={1}&client_secret={2}&redirect_uri={3}&grant_type=authorization_code",
                                            authCode, clientid, secret, redirectURI);
            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                string responseString = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<AuthResponse>(responseString);
            }
        }
    }
}
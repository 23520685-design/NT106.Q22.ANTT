using MiniSocialApp.Config;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MiniSocialApp.Services
{
    public class StorageService
    {
        // ✅ FIX 6: Dùng static readonly HttpClient → tránh socket exhaustion
        private static readonly HttpClient _client = new HttpClient();

        private readonly string SUPABASE_URL = SupabaseConfig.Url;
        private readonly string API_KEY      = SupabaseConfig.ApiKey;
        private readonly string BUCKET       = SupabaseConfig.Bucket;

        public StorageService()
        {
            // Headers cần set 1 lần, kiểm tra trước khi add tránh duplicate
            if (!_client.DefaultRequestHeaders.Contains("apikey"))
            {
                _client.DefaultRequestHeaders.Add("apikey", API_KEY);
                _client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", API_KEY);
            }
        }

        private string GetMimeType(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLower();

            switch (ext)
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".webp":
                    return "image/webp";
                default:
                    return "application/octet-stream";
            }
        }

        public async Task<string> UploadImage(string filePath)
        {
            var fileBytes = File.ReadAllBytes(filePath);

            var content = new ByteArrayContent(fileBytes);
            var mimeType = GetMimeType(filePath);
            content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

            string ext = Path.GetExtension(filePath).ToLower();
            if (string.IsNullOrEmpty(ext)) ext = ".jpg";

            string fileName = Guid.NewGuid().ToString() + ext;

            var response = await _client.PostAsync(
                $"{SUPABASE_URL}/storage/v1/object/{BUCKET}/{fileName}",
                content
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception("Upload failed: " + error);
            }

            return $"{SUPABASE_URL}/storage/v1/object/public/{BUCKET}/{fileName}";
        }
    }
}

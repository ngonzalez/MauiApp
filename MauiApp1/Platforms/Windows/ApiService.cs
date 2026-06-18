using System.Net.Http.Headers;

namespace MauiApp1.Platforms.Windows
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;

        private readonly HttpClient _httpClientStreamingService;
        public ApiService()
        {
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri("https://link12.ddns.net:4040")
            };

            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            _httpClientStreamingService = new HttpClient()
            {
                BaseAddress = new Uri("https://link12.ddns.net:5050")
            };

            _httpClientStreamingService.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
        }

        public async Task<(int, String)> getUploads(string ids)
        {
            var response = await _httpClient.GetAsync("/upload" + ids);
            string json = await response.Content.ReadAsStringAsync();
            int status = (int)response.StatusCode;
            return (status, json);
        }

        public async Task<(int, String)> CreatePostAsync(byte[] body)
        {
            ByteArrayContent content = new ByteArrayContent(body);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            content.Headers.ContentLength = body.Length;
            var response = await _httpClient.PostAsync("/upload", content);
            string json = await response.Content.ReadAsStringAsync();
            int status = (int)response.StatusCode;
            return (status, json);
        }

        public async Task<(int, String)> getVideoStream(string id)
        {
            var response = await _httpClientStreamingService.GetAsync("/video_files/" + id);
            var json = await response.Content.ReadAsStringAsync();
            int status = (int)response.StatusCode;
            return (status, json);
        }

        public async Task<(int, String)> getAudioStream(string id)
        {
            var response = await _httpClientStreamingService.GetAsync("/audio_files/" + id);
            var json = await response.Content.ReadAsStringAsync();
            int status = (int)response.StatusCode;
            return (status, json);
        }

        public async Task<(int, String)> PublishFolders(byte[] body)
        {
            ByteArrayContent content = new ByteArrayContent(body);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            content.Headers.ContentLength = body.Length;
            var response = await _httpClient.PostAsync("/folders/publish", content);
            string json = await response.Content.ReadAsStringAsync();
            int status = (int)response.StatusCode;
            return (status, json);
        }

        public async Task<(int, String)> UnpublishFolders(byte[] body)
        {
            ByteArrayContent content = new ByteArrayContent(body);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            content.Headers.ContentLength = body.Length;
            var response = await _httpClient.PostAsync("/folders/unpublish", content);
            string json = await response.Content.ReadAsStringAsync();
            int status = (int)response.StatusCode;
            return (status, json);
        }

        public async Task<(int, String)> ArchiveFolders(byte[] body)
        {
            ByteArrayContent content = new ByteArrayContent(body);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            content.Headers.ContentLength = body.Length;
            var response = await _httpClient.PostAsync("/folders/archive", content);
            string json = await response.Content.ReadAsStringAsync();
            int status = (int)response.StatusCode;
            return (status, json);
        }

        public async Task<(int, String)> UnarchiveFolders(byte[] body)
        {
            ByteArrayContent content = new ByteArrayContent(body);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            content.Headers.ContentLength = body.Length;
            var response = await _httpClient.PostAsync("/folders/unarchive", content);
            string json = await response.Content.ReadAsStringAsync();
            int status = (int)response.StatusCode;
            return (status, json);
        }

        public async Task<(int, String)> DeleteFolders(byte[] body)
        {
            ByteArrayContent content = new ByteArrayContent(body);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            content.Headers.ContentLength = body.Length;
            var response = await _httpClient.PostAsync("/folders/delete", content);
            string json = await response.Content.ReadAsStringAsync();
            int status = (int)response.StatusCode;
            return (status, json);
        }

        public async Task<(int, String)> DeleteAttachments(byte[] body)
        {
            ByteArrayContent content = new ByteArrayContent(body);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            content.Headers.ContentLength = body.Length;
            var response = await _httpClient.PostAsync("/attachments/delete", content);
            string json = await response.Content.ReadAsStringAsync();
            int status = (int)response.StatusCode;
            return (status, json);
        }

        public async Task<(int, String)> CreateEvent(byte[] body)
        {
            ByteArrayContent content = new ByteArrayContent(body);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            content.Headers.ContentLength = body.Length;
            var response = await _httpClient.PostAsync("/event", content);
            string json = await response.Content.ReadAsStringAsync();
            int status = (int)response.StatusCode;
            return (status, json);
        }
    }
}

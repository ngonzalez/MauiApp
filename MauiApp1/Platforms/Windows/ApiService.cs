using System.Net.Http.Headers;

namespace MauiApp1.Platforms.Windows
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;

        private readonly HttpClient _httpClientStreamingService;

        private readonly DirectUploadClient _directUploadClient;

        public ApiService(AppShellViewModel appShellViewModel)
        {
            _httpClient = new HttpClient(new BearerTokenHandler(appShellViewModel))
            {
                BaseAddress = new Uri("https://api.appshare.site")
            };

            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            _httpClientStreamingService = new HttpClient()
            {
                BaseAddress = new Uri("https://stream.appshare.site")
            };

            _httpClientStreamingService.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            // The files go to the storage's signed URLs: no session token, and
            // no timeout (the default 100 seconds would stop a large video)
            var storageClient = new HttpClient()
            {
                Timeout = Timeout.InfiniteTimeSpan
            };

            _directUploadClient = new DirectUploadClient(_httpClient, storageClient);
        }

        public async Task<(int, String)> getUploads(Guid accountUuid, string ids)
        {
            var accountUuidStr = Convert.ToString(accountUuid);
            var response = await _httpClient.GetAsync("/upload/" + accountUuidStr + "/" + ids);
            string json = await response.Content.ReadAsStringAsync();
            int status = (int)response.StatusCode;
            return (status, json);
        }

        public Task<Guid> UploadFileAsync(UploadFile uploadFile)
        {
            return _directUploadClient.UploadAsync(uploadFile.filePath, uploadFile.source, uploadFile.mimeType, uploadFile.createdAt, uploadFile.updatedAt);
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

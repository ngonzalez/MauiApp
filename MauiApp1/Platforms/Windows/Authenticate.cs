using System.Net.Http.Headers;
using Windows.Web.Http;
using HttpClient = System.Net.Http.HttpClient;

namespace MauiApp1.Platforms.Windows
{
    public class Authenticate : IAuthenticate
    {

        public User _user;

        private readonly HttpClient _httpClient;

        private readonly AppShellViewModel _appShellViewModel;

        public Authenticate(AppShellViewModel appShellViewModel)
        {
            _appShellViewModel = appShellViewModel;
            _httpClient = new HttpClient(new BearerTokenHandler(appShellViewModel))
            {
                BaseAddress = new Uri("https://api.appshare.site")
            };
        }

        public async Task<User> getCurrentUser()
        {
            return _appShellViewModel.CurrentUser;
        }

        public async Task<bool> setCurrentUser(User user)
        {
            _appShellViewModel.CurrentUser = user;
            CurrentUserAvatar.Shared.Update(user);
            return true;
        }

        public async Task<string?> getToken()
        {
            return _appShellViewModel.Token;
        }

        public async Task<bool> setToken(string? token)
        {
            _appShellViewModel.Token = token;
            return true;
        }

        public async Task<(int, String)> newSession(Dictionary<string, string> values)
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var content = new FormUrlEncodedContent(values);
            var response = await _httpClient.PostAsync("/session", content);
            var json = await response.Content.ReadAsStringAsync();
            int status = (int)response.StatusCode;
            return (status, json);
        }

        public async Task<(int, String)> deleteSession()
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await _httpClient.DeleteAsync("/session");
            var json = await response.Content.ReadAsStringAsync();
            int status = (int)response.StatusCode;
            return (status, json);
        }

        public async Task<(int, String)> updateAccount(Dictionary<string, string> values)
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var content = new FormUrlEncodedContent(values);
            var response = await _httpClient.PutAsync("/user", content);
            var json = await response.Content.ReadAsStringAsync();
            int status = (int)response.StatusCode;
            return (status, json);
        }

        public async Task<(int, String)> updatePassword(Dictionary<string, string> values)
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var content = new FormUrlEncodedContent(values);
            var response = await _httpClient.PutAsync("/password", content);
            var json = await response.Content.ReadAsStringAsync();
            int status = (int)response.StatusCode;
            return (status, json);
        }

        public async Task<(int, String)> resetPassword(Dictionary<string, string> values)
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var content = new FormUrlEncodedContent(values);
            var response = await _httpClient.PostAsync("/password", content);
            var json = await response.Content.ReadAsStringAsync();
            int status = (int)response.StatusCode;
            return (status, json);
        }

        public async Task<(int, String)> updateEmailAddress(Dictionary<string, string> values)
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var content = new FormUrlEncodedContent(values);
            var response = await _httpClient.PutAsync("/email", content);
            var json = await response.Content.ReadAsStringAsync();
            int status = (int)response.StatusCode;
            return (status, json);
        }

        // A plain multipart body (not gzipped), the file in the "avatar" field
        public async Task<(int, String)> updateAvatar(byte[] picture, string fileName, string contentType)
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            using var content = new MultipartFormDataContent();
            var file = new ByteArrayContent(picture);
            file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Add(file, "avatar", fileName);
            var response = await _httpClient.PutAsync("/user/avatar", content);
            var json = await response.Content.ReadAsStringAsync();
            int status = (int)response.StatusCode;
            return (status, json);
        }

        public async Task<(int, String)> setAvatarPreset(string name)
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var body = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string> { { "preset", name } });
            using var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync("/user/avatar", content);
            var json = await response.Content.ReadAsStringAsync();
            int status = (int)response.StatusCode;
            return (status, json);
        }

        public async Task<(int, String)> deleteAvatar()
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await _httpClient.DeleteAsync("/user/avatar");
            var json = await response.Content.ReadAsStringAsync();
            int status = (int)response.StatusCode;
            return (status, json);
        }

        public async Task<(int, String)> getAvatarPresets()
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await _httpClient.GetAsync("/avatars/presets");
            var json = await response.Content.ReadAsStringAsync();
            int status = (int)response.StatusCode;
            return (status, json);
        }
    }
}

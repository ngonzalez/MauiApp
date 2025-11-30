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
            _httpClient = new HttpClient()
            {
                //BaseAddress = new Uri("http://192.168.1.11:3000")
                BaseAddress = new Uri("https://link12.ddns.net:4040")
            };
        }

        public async Task<User> getCurrentUser()
        {
            return _appShellViewModel.CurrentUser;
        }

        public async Task<bool> setCurrentUser(User user)
        {
            _appShellViewModel.CurrentUser = user;
            return true;
        }

        public async Task<int> getSessionID()
        {
            return _appShellViewModel.SessionID;
        }

        public async Task<bool> setSessionID(int sessionID)
        {
            _appShellViewModel.SessionID = sessionID;
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
            var response = await _httpClient.PutAsync("/account", content);
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

        public async Task<(int, String)> updateEmailAddress(Dictionary<string, string> values)
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var content = new FormUrlEncodedContent(values);
            var response = await _httpClient.PutAsync("/email", content);
            var json = await response.Content.ReadAsStringAsync();
            int status = (int)response.StatusCode;
            return (status, json);
        }
    }
}

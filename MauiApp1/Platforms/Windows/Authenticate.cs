using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Windows.Media.Protection.PlayReady;

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
        public async Task<String> newSession(Dictionary<string, string> values)
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var content = new FormUrlEncodedContent(values);
            var response = await _httpClient.PostAsync("/session", content);
            var json = await response.Content.ReadAsStringAsync();
            return json;
        }
    }
}

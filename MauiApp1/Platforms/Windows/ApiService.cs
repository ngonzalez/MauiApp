using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Windows.Media.Protection.PlayReady;

namespace MauiApp1.Platforms.Windows
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        public ApiService()
        {
            // Create a CookieContainer to store cookies
            var cookieContainer = new CookieContainer();

            // Configure HttpClientHandler with the cookie container
            var httpClientHandler = new HttpClientHandler
            {
                CookieContainer = cookieContainer,
                UseCookies = true
            };

            _httpClient = new HttpClient(httpClientHandler)
            {
                BaseAddress = new Uri("https://link12.ddns.net:4040")
            };
        }
        public async Task<string> CreatePostAsync(System.Net.Http.StringContent jsonContent)
        {
            var response = await _httpClient.PostAsync("/upload", jsonContent);
            var json = await response.Content.ReadAsStringAsync();
            return json;
        }
    }
}

using System;
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
        private readonly JsonSerializerOptions _jsonOptions;
        public ApiService()
        {
            _httpClient = new HttpClient
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

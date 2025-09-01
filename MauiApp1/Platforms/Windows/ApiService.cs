using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri("http://192.168.1.11:3000")
            };
        }
        public async Task<string> CreatePostAsync(System.Net.Http.StringContent content)
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await _httpClient.PostAsync("/upload", content);
            var json = await response.Content.ReadAsStringAsync();
            return json;
        }
    }
}

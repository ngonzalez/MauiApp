using Microsoft.Maui.Devices.Sensors;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Windows.Foundation.Collections;
using Windows.Media.Protection.PlayReady;

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
                //BaseAddress = new Uri("http://192.168.1.11:3000")
                BaseAddress = new Uri("https://link12.ddns.net:4040")
            };

            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            _httpClientStreamingService = new HttpClient()
            {
                //BaseAddress = new Uri("http://192.168.1.11:3000")
                BaseAddress = new Uri("https://link12.ddns.net:5050")
            };

            _httpClientStreamingService.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
        }

        public async Task<string> getVideoStream(string id)
        {
            var httpResponse = await _httpClientStreamingService.GetAsync("/video_files/" + id);
            string response = await httpResponse.Content.ReadAsStringAsync();
            return response;
        }

        public async Task<string> GetAllUploads(string ids)
        {
            var httpResponse = await _httpClient.GetAsync("/upload" + ids);
            string response = await httpResponse.Content.ReadAsStringAsync();
            return response;
        }

        public async Task<string> CreatePostAsync(byte[] body)
        {
            ByteArrayContent content = new ByteArrayContent(body);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            content.Headers.ContentLength = body.Length;
            var httpResponse = await _httpClient.PostAsync("/upload", content);
            string response = await httpResponse.Content.ReadAsStringAsync();
            return response;
        }
    }
}

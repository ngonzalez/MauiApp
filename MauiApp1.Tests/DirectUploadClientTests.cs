using System.Net;
using System.Text;
using System.Text.Json;
using MauiApp1;
using Xunit;

namespace MauiApp1.Tests
{
    public class DirectUploadClientTests : IDisposable
    {
        private const string Uuid = "0f8fad5b-d9cb-469f-a165-70867728950e";
        private const string SignedUrl = "https://storage.example.test/container/key?sig=abc";

        private readonly string _filePath;
        private readonly FakeHandler _api = new FakeHandler();
        private readonly FakeHandler _storage = new FakeHandler();
        private readonly DirectUploadClient _client;

        public DirectUploadClientTests()
        {
            _filePath = Path.Combine(Path.GetTempPath(), "direct-upload-" + Guid.NewGuid() + ".png");
            File.WriteAllText(_filePath, "hello");

            var api = new HttpClient(_api) { BaseAddress = new Uri("https://api.example.test") };
            api.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "session-token");
            _client = new DirectUploadClient(api, new HttpClient(_storage));
        }

        public void Dispose()
        {
            File.Delete(_filePath);
        }

        private static HttpResponseMessage Json(HttpStatusCode status, object body)
        {
            return new HttpResponseMessage(status) { Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json") };
        }

        private static HttpResponseMessage Created()
        {
            return Json(HttpStatusCode.Created, new
            {
                uuid = Uuid,
                signedBlobId = "signed-blob",
                directUpload = new
                {
                    url = SignedUrl,
                    headers = new Dictionary<string, string>
                    {
                        ["Content-Type"] = "image/png",
                        ["Content-MD5"] = "XUFAKrxLKna5cZ2REBfFkg==",
                        ["x-ms-blob-type"] = "BlockBlob",
                    },
                },
            });
        }

        private Task<Guid> Upload()
        {
            return _client.UploadAsync(_filePath, "folder", "image/png", new DateTime(2026, 10, 1, 10, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 2, 10, 0, 0, DateTimeKind.Utc));
        }

        [Fact]
        public async Task Md5IsTheBase64DigestOfTheFile()
        {
            // printf hello | openssl dgst -md5 -binary | base64
            Assert.Equal("XUFAKrxLKna5cZ2REBfFkg==", await DirectUploadClient.Md5Base64Async(_filePath));
        }

        [Fact]
        public async Task UploadsTheWholeFileInOnePutBetweenTheTwoApiCalls()
        {
            _api.Responses.Enqueue(Created());
            _storage.Responses.Enqueue(new HttpResponseMessage(HttpStatusCode.Created));
            _api.Responses.Enqueue(Json(HttpStatusCode.Accepted, new { uuid = Uuid, status = "processing" }));

            Assert.Equal(Guid.Parse(Uuid), await Upload());

            Assert.Equal(2, _api.Requests.Count);
            FakeRequest create = _api.Requests[0];
            Assert.Equal("POST", create.Method);
            Assert.Equal("https://api.example.test/upload/direct", create.Uri);
            JsonElement details = JsonDocument.Parse(create.Body).RootElement;
            Assert.Equal(_filePath, details.GetProperty("filePath").GetString());
            Assert.Equal("folder", details.GetProperty("source").GetString());
            Assert.Equal("image/png", details.GetProperty("mimeType").GetString());
            Assert.Equal(5, details.GetProperty("byteSize").GetInt64());
            Assert.Equal("XUFAKrxLKna5cZ2REBfFkg==", details.GetProperty("checksum").GetString());
            Assert.Equal("2026-10-01T10:00:00.0000000Z", details.GetProperty("createdAt").GetString());

            FakeRequest put = Assert.Single(_storage.Requests);
            Assert.Equal("PUT", put.Method);
            Assert.Equal(SignedUrl, put.Uri);
            Assert.Equal("hello", Encoding.UTF8.GetString(put.Body));
            Assert.Equal("image/png", put.Headers["Content-Type"]);
            Assert.Equal("XUFAKrxLKna5cZ2REBfFkg==", put.Headers["Content-MD5"]);
            Assert.Equal("BlockBlob", put.Headers["x-ms-blob-type"]);
            Assert.Equal("5", put.Headers["Content-Length"]);

            FakeRequest complete = _api.Requests[1];
            Assert.Equal("https://api.example.test/upload/direct/" + Uuid + "/complete", complete.Uri);
            Assert.Equal("signed-blob", JsonDocument.Parse(complete.Body).RootElement.GetProperty("signedBlobId").GetString());
        }

        [Fact]
        public async Task TheStorageRequestCarriesNoSessionToken()
        {
            _api.Responses.Enqueue(Created());
            _storage.Responses.Enqueue(new HttpResponseMessage(HttpStatusCode.Created));
            _api.Responses.Enqueue(Json(HttpStatusCode.Accepted, new { uuid = Uuid, status = "processing" }));

            await Upload();

            Assert.Equal("Bearer session-token", _api.Requests[0].Headers["Authorization"]);
            Assert.False(_storage.Requests[0].Headers.ContainsKey("Authorization"));
        }

        [Fact]
        public async Task ARefusedFileIsNotSent()
        {
            _api.Responses.Enqueue(Json(HttpStatusCode.UnprocessableEntity, new { message = "This file type is not supported" }));

            var error = await Assert.ThrowsAsync<DirectUploadException>(Upload);

            Assert.Equal(422, error.StatusCode);
            Assert.Equal("This file type is not supported", error.Message);
            Assert.Empty(_storage.Requests);
        }

        [Fact]
        public async Task AFailedPutIsNotCompleted()
        {
            _api.Responses.Enqueue(Created());
            _storage.Responses.Enqueue(new HttpResponseMessage(HttpStatusCode.Forbidden) { Content = new StringContent("<Error><Code>AuthenticationFailed</Code></Error>") });

            var error = await Assert.ThrowsAsync<DirectUploadException>(Upload);

            Assert.Equal(403, error.StatusCode);
            Assert.Single(_api.Requests);
        }

        [Fact]
        public async Task AFailedCompletionIsReported()
        {
            _api.Responses.Enqueue(Created());
            _storage.Responses.Enqueue(new HttpResponseMessage(HttpStatusCode.Created));
            _api.Responses.Enqueue(Json(HttpStatusCode.UnprocessableEntity, new { message = "The file is not in the storage yet" }));

            var error = await Assert.ThrowsAsync<DirectUploadException>(Upload);

            Assert.Equal("The file is not in the storage yet", error.Message);
        }

        [Fact]
        public async Task AnExpiredSessionIsReported()
        {
            _api.Responses.Enqueue(new HttpResponseMessage(HttpStatusCode.Unauthorized));

            var error = await Assert.ThrowsAsync<DirectUploadException>(Upload);

            Assert.Equal(401, error.StatusCode);
        }
    }

    public record FakeRequest(string Method, string Uri, Dictionary<string, string> Headers, byte[] Body);

    // Records the requests and answers them in order
    public class FakeHandler : HttpMessageHandler
    {
        public Queue<HttpResponseMessage> Responses { get; } = new Queue<HttpResponseMessage>();

        public List<FakeRequest> Requests { get; } = new List<FakeRequest>();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var headers = request.Headers.ToDictionary(header => header.Key, header => string.Join(",", header.Value));
            byte[] body = Array.Empty<byte>();
            if (request.Content != null)
            {
                foreach (var header in request.Content.Headers)
                {
                    headers[header.Key] = string.Join(",", header.Value);
                }
                body = await request.Content.ReadAsByteArrayAsync(cancellationToken);
            }
            Requests.Add(new FakeRequest(request.Method.Method, request.RequestUri!.ToString(), headers, body));
            return Responses.Dequeue();
        }
    }
}

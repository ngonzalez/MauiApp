using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MauiApp1
{
    // Uploads a file straight to the storage, in one PUT and without chunks
    // (the backend's /upload/direct, with the signed-in session's token):
    // 1. POST /upload/direct: the file's details -> a signed URL
    // 2. PUT the file to that URL, streamed from the disk (up to 5000 MiB)
    // 3. POST /upload/direct/{uuid}/complete: the backend attaches it
    // Every response is checked: a failure throws DirectUploadException.
    // No MAUI dependency, so MauiApp1.Tests compiles this file.
    public class DirectUploadClient
    {
        // The backend, with the session token (BearerTokenHandler)
        private readonly HttpClient _api;

        // The storage: no token, the signed URL is the authorization (Azure
        // refuses a request that has both). Its timeout must allow big files.
        private readonly HttpClient _storage;

        public DirectUploadClient(HttpClient api, HttpClient storage)
        {
            _api = api;
            _storage = storage;
        }

        // Upload one file; returns the upload's uuid (the backend attaches
        // the file in the background)
        public async Task<Guid> UploadAsync(string filePath, string source, string mimeType, DateTime createdAt, DateTime updatedAt, CancellationToken cancellationToken = default)
        {
            long byteSize = new FileInfo(filePath).Length;
            string checksum = await Md5Base64Async(filePath, cancellationToken);

            using JsonDocument created = await PostAsync("/upload/direct", new Dictionary<string, object>
            {
                ["filePath"] = filePath,
                ["source"] = source,
                ["mimeType"] = mimeType,
                ["byteSize"] = byteSize,
                ["checksum"] = checksum,
                ["createdAt"] = createdAt.ToString("o", CultureInfo.InvariantCulture),
                ["updatedAt"] = updatedAt.ToString("o", CultureInfo.InvariantCulture),
            }, cancellationToken);

            JsonElement root = created.RootElement;
            string uuid = root.GetProperty("uuid").GetString()!;
            string signedBlobId = root.GetProperty("signedBlobId").GetString()!;
            JsonElement directUpload = root.GetProperty("directUpload");

            await using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 1024 * 1024, useAsync: true))
            using (var request = new HttpRequestMessage(HttpMethod.Put, directUpload.GetProperty("url").GetString()) { Content = new StreamContent(file) })
            {
                foreach (JsonProperty header in directUpload.GetProperty("headers").EnumerateObject())
                {
                    string value = header.Value.GetString() ?? "";
                    // Content-Type and Content-MD5 are content headers
                    if (!request.Headers.TryAddWithoutValidation(header.Name, value))
                    {
                        request.Content.Headers.TryAddWithoutValidation(header.Name, value);
                    }
                }
                request.Content.Headers.ContentLength = byteSize;

                using HttpResponseMessage response = await _storage.SendAsync(request, cancellationToken);
                await EnsureSuccessAsync(response, cancellationToken);
            }

            using JsonDocument completed = await PostAsync("/upload/direct/" + uuid + "/complete",
                new Dictionary<string, object> { ["signedBlobId"] = signedBlobId }, cancellationToken);
            return Guid.Parse(completed.RootElement.GetProperty("uuid").GetString()!);
        }

        // Base64 MD5 of a file, read as a stream: a large video never sits in memory
        public static async Task<string> Md5Base64Async(string filePath, CancellationToken cancellationToken = default)
        {
            await using FileStream stream = File.OpenRead(filePath);
            return Convert.ToBase64String(await MD5.HashDataAsync(stream, cancellationToken));
        }

        private async Task<JsonDocument> PostAsync(string path, Dictionary<string, object> body, CancellationToken cancellationToken)
        {
            using var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            using HttpResponseMessage response = await _api.PostAsync(path, content, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
            return await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        }

        // A status outside 2xx throws, with the server's "message" when it sends one
        private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            string body = await response.Content.ReadAsStringAsync(cancellationToken);
            string? message = null;
            try
            {
                using JsonDocument json = JsonDocument.Parse(body);
                if (json.RootElement.ValueKind == JsonValueKind.Object && json.RootElement.TryGetProperty("message", out JsonElement value))
                {
                    message = value.GetString();
                }
            }
            catch (JsonException)
            {
                // Azure answers in XML
            }
            throw new DirectUploadException((int)response.StatusCode, message ?? response.ReasonPhrase ?? "Upload failed");
        }
    }

    public class DirectUploadException : Exception
    {
        public int StatusCode { get; }

        public DirectUploadException(int statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}

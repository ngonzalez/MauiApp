using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MauiApp1
{
    // Invites someone by email to join the signed-in user's account:
    // POST /invitations {"emailAddress": "..."}, with the HttpClient of
    // ApiService (its base URL and the session's token, BearerTokenHandler),
    // like PUT /password. The address is checked first with the backend's rule.
    //   200 {"message": "Invitation sent to anna@example.com"}
    //   422 {"message": "...", "errors": ["This address already has a user"]}
    //   401 not signed in, 429 too many invitations
    // A network failure throws (HttpRequestException).
    // No MAUI dependency, so MauiApp1.Tests compiles this file.
    public class InvitationClient
    {
        public const string Path = "/invitations";

        // The backend's AuthenticationConcern::EMAIL_REGEX,
        // /\A[\w+\-.]+@[a-z\d\-]+(\.[a-z]+)*\.[a-z]+\z/i (\w is ASCII in Ruby)
        private static readonly Regex EmailFormat = new Regex(
            @"^[A-Za-z0-9_+\-.]+@[A-Za-z0-9\-]+(\.[A-Za-z]+)*\.[A-Za-z]+\z", RegexOptions.CultureInvariant);

        public enum Problem { EmailAddressRequired, EmailAddressInvalid }

        public enum Outcome { Sent, Refused, NotSignedIn, TooManyRequests, Failed }

        // A response: its outcome, and the backend's message and errors
        public record Result(Outcome Outcome, int StatusCode, string? Message, IReadOnlyList<string> Errors)
        {
            // What to show for a refusal: the errors, or the message without any
            public IReadOnlyList<string> Messages =>
                Errors.Count > 0 ? Errors : Message != null ? new[] { Message } : Array.Empty<string>();
        }

        // The backend, with the session token (BearerTokenHandler)
        private readonly HttpClient _api;

        public InvitationClient(HttpClient api)
        {
            _api = api;
        }

        // What is wrong with an address before it is sent; null when it can go
        public static Problem? Check(string? emailAddress)
        {
            string address = (emailAddress ?? "").Trim();
            if (address.Length == 0)
            {
                return Problem.EmailAddressRequired;
            }
            if (!EmailFormat.IsMatch(address))
            {
                return Problem.EmailAddressInvalid;
            }
            return null;
        }

        // The resource key of each problem's message (AppResources)
        public static string MessageKey(Problem problem)
        {
            return problem.ToString();
        }

        // POST /invitations with the address (trimmed) as JSON
        public static HttpRequestMessage BuildRequest(string emailAddress)
        {
            string body = JsonSerializer.Serialize(new Dictionary<string, string> { ["emailAddress"] = emailAddress.Trim() });
            return new HttpRequestMessage(HttpMethod.Post, Path)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
        }

        public async Task<Result> SendAsync(string emailAddress, CancellationToken cancellationToken = default)
        {
            using HttpRequestMessage request = BuildRequest(emailAddress);
            using HttpResponseMessage response = await _api.SendAsync(request, cancellationToken);
            return Read((int)response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken));
        }

        // A response's status and body; a body that can't be read (an HTML
        // error page) gives no message and no errors
        public static Result Read(int statusCode, string json)
        {
            Outcome outcome = statusCode switch
            {
                >= 200 and < 300 => Outcome.Sent,
                401 => Outcome.NotSignedIn,
                422 => Outcome.Refused,
                429 => Outcome.TooManyRequests,
                _ => Outcome.Failed,
            };
            string? message = null;
            var errors = new List<string>();
            try
            {
                using var document = JsonDocument.Parse(json);
                JsonElement root = document.RootElement;
                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("message", out JsonElement text) && text.ValueKind == JsonValueKind.String)
                    {
                        message = text.GetString();
                    }
                    if (root.TryGetProperty("errors", out JsonElement list) && list.ValueKind == JsonValueKind.Array)
                    {
                        errors.AddRange(list.EnumerateArray()
                                            .Where(error => error.ValueKind == JsonValueKind.String)
                                            .Select(error => error.GetString()!));
                    }
                }
            }
            catch (JsonException)
            {
            }
            return new Result(outcome, statusCode, string.IsNullOrEmpty(message) ? null : message, errors);
        }
    }
}

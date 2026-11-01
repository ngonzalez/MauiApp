using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;
using static MauiApp1.InvitationClient;

namespace MauiApp1.Tests
{
    // Inviting a user by email: the address check, POST /invitations and its responses
    public class InvitationClientTests
    {
        private readonly FakeHandler _api = new FakeHandler();
        private readonly InvitationClient _client;

        public InvitationClientTests()
        {
            var api = new HttpClient(_api) { BaseAddress = new Uri("https://api.example.test") };
            api.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "session-token");
            _client = new InvitationClient(api);
        }

        private static HttpResponseMessage Json(HttpStatusCode status, string body)
        {
            return new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
        }

        [Theory]
        [InlineData("anna@example.com")]
        [InlineData("Anna.Smith+test@mail.example.co.uk")]
        [InlineData("  anna_smith-1@example-mail.org  ")]
        public void AcceptsAValidAddress(string address)
        {
            Assert.Null(Check(address));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void RefusesAnEmptyAddress(string? address)
        {
            Assert.Equal(Problem.EmailAddressRequired, Check(address));
        }

        // The backend's EMAIL_REGEX refuses these too
        [Theory]
        [InlineData("anna")]
        [InlineData("anna@")]
        [InlineData("@example.com")]
        [InlineData("anna@example")]
        [InlineData("anna@example.c0m")]
        [InlineData("anna smith@example.com")]
        [InlineData("anna@exa_mple.com")]
        [InlineData("anna@example.com.")]
        [InlineData("anna@@example.com")]
        [InlineData("anné@example.com")]
        public void RefusesAnInvalidAddress(string address)
        {
            Assert.Equal(Problem.EmailAddressInvalid, Check(address));
        }

        [Fact]
        public void MessageKeys()
        {
            Assert.Equal("EmailAddressRequired", MessageKey(Problem.EmailAddressRequired));
            Assert.Equal("EmailAddressInvalid", MessageKey(Problem.EmailAddressInvalid));
        }

        [Fact]
        public async Task PostsTheTrimmedAddressAsJsonWithTheSessionToken()
        {
            _api.Responses.Enqueue(Json(HttpStatusCode.OK, """{"message":"Invitation sent to anna@example.com"}"""));

            await _client.SendAsync(" anna@example.com ");

            FakeRequest request = Assert.Single(_api.Requests);
            Assert.Equal("POST", request.Method);
            Assert.Equal("https://api.example.test/invitations", request.Uri);
            Assert.Equal("Bearer session-token", request.Headers["Authorization"]);
            Assert.StartsWith("application/json", request.Headers["Content-Type"]);
            using var body = JsonDocument.Parse(request.Body);
            Assert.Equal("anna@example.com", body.RootElement.GetProperty("emailAddress").GetString());
            Assert.Single(body.RootElement.EnumerateObject());
        }

        [Fact]
        public async Task ReadsASentInvitation()
        {
            _api.Responses.Enqueue(Json(HttpStatusCode.OK, """{"message":"Invitation sent to anna@example.com"}"""));

            Result result = await _client.SendAsync("anna@example.com");

            Assert.Equal(Outcome.Sent, result.Outcome);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("Invitation sent to anna@example.com", result.Message);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task ReadsTheErrorsOfARefusal()
        {
            _api.Responses.Enqueue(Json(HttpStatusCode.UnprocessableEntity,
                """{"message":"The invitation was not sent","errors":["This address already has a user","Your plan has no room for another user"]}"""));

            Result result = await _client.SendAsync("anna@example.com");

            Assert.Equal(Outcome.Refused, result.Outcome);
            Assert.Equal("The invitation was not sent", result.Message);
            Assert.Equal(new[] { "This address already has a user", "Your plan has no room for another user" }, result.Messages);
        }

        [Theory]
        [InlineData("""{"message":"The invitation was not sent"}""")]
        [InlineData("""{"message":"The invitation was not sent","errors":[]}""")]
        [InlineData("""{"message":"The invitation was not sent","errors":null}""")]
        public void ARefusalWithoutErrorsShowsItsMessage(string json)
        {
            Result result = Read(422, json);

            Assert.Equal(Outcome.Refused, result.Outcome);
            Assert.Equal(new[] { "The invitation was not sent" }, result.Messages);
        }

        [Fact]
        public void ReadsTooManyInvitations()
        {
            Assert.Equal(Outcome.TooManyRequests, Read(429, """{"message":"Too many requests"}""").Outcome);
            Assert.Equal(Outcome.TooManyRequests, Read(429, "").Outcome);
        }

        [Fact]
        public void ReadsNotSignedIn()
        {
            Assert.Equal(Outcome.NotSignedIn, Read(401, """{"message":"Not signed in"}""").Outcome);
        }

        [Theory]
        [InlineData(500, "<html>Internal Server Error</html>")]
        [InlineData(502, "")]
        [InlineData(422, "<html>")]
        public void AnAnswerItCantReadHasNoMessage(int status, string body)
        {
            Result result = Read(status, body);

            Assert.Null(result.Message);
            Assert.Empty(result.Messages);
            Assert.Equal(status == 422 ? Outcome.Refused : Outcome.Failed, result.Outcome);
        }

        [Fact]
        public async Task ANetworkFailureThrows()
        {
            var client = new InvitationClient(new HttpClient(new FailingHandler()) { BaseAddress = new Uri("https://api.example.test") });

            await Assert.ThrowsAsync<HttpRequestException>(() => client.SendAsync("anna@example.com"));
        }

        private class FailingHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw new HttpRequestException("No such host is known.");
            }
        }
    }
}

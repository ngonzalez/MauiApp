using System.Net.Http.Headers;

namespace MauiApp1.Platforms.Windows
{
    // Adds "Authorization: Bearer <token>" to the backend requests once signed
    // in: the backend refuses them without it (401)
    public class BearerTokenHandler : DelegatingHandler
    {
        private readonly AppShellViewModel _appShellViewModel;

        public BearerTokenHandler(AppShellViewModel appShellViewModel)
        {
            _appShellViewModel = appShellViewModel;
            InnerHandler = new HttpClientHandler();
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = _appShellViewModel.Token;
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}

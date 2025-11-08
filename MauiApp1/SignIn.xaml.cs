using Microsoft.Toolkit.Uwp.Notifications;
using System.Text.Json;

namespace MauiApp1;

public class NewSessionResponse
{

    public User user { get; set; }

    public string message { get; set; }

    public int sessionId { get; set; }
}

public partial class SignInPage : ContentPage
{
    private readonly IAuthenticate _authenticate;

    private string EmailAddress;

    private string Password;

    private readonly AppShellViewModel _appShellViewModel;

    public SignInPage(IAuthenticate authenticate, AppShellViewModel appShellViewModel)
    {
        _authenticate = authenticate;
        _appShellViewModel = appShellViewModel;
        InitializeComponent();
        BindingContext = this;
    }

    private async void OnSignInClicked(object sender, EventArgs e)
    {
        var values = new Dictionary<string, string> {
            { "emailAddress", EmailAddress },
            { "password", Password },
        };

        var response = await _authenticate.newSession(values);

        NewSessionResponse jsonResponse = JsonSerializer.Deserialize<NewSessionResponse>(response);

        ToastNotificationManagerCompat.History.Clear();

        if (jsonResponse?.user != null)
        {
            if (jsonResponse?.user?.id != null)
            {
                new ToastContentBuilder()
                    .AddText(string.Concat(jsonResponse.message))
                    .Show();

                await _authenticate.setSessionID(jsonResponse.sessionId);

                await _authenticate.setCurrentUser(jsonResponse.user);

                await Shell.Current.GoToAsync("account");

            }
            else if (jsonResponse.user.errors.Length > 0)
            {
                new ToastContentBuilder()
                    .AddText(string.Concat(jsonResponse.user.errors))
                    .Show();
            }
            else if (jsonResponse?.message != null)
            {
                new ToastContentBuilder()
                    .AddText(string.Concat(jsonResponse.message))
                    .Show();
            }
        }
    }

    private async void OnEmailAddressCompleted(object sender, EventArgs e)
    {
        EmailAddress = ((Entry)sender).Text;
    }

    private async void OnPasswordCompleted(object sender, EventArgs e)
    {
        Password = ((Entry)sender).Text;
    }
}

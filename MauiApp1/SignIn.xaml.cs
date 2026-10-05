using Microsoft.Toolkit.Uwp.Notifications;
using System.Text.Json;


namespace MauiApp1;

public class NewSessionResponse
{

    public User user { get; set; }

    public string message { get; set; }

    // Sent back as "Authorization: Bearer <token>" (BearerTokenHandler)
    public string? token { get; set; }

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

    public async void OnResetPasswordClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("resetpasswordpage");
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("registerpage");
    }

    private async void OnSignInClicked(object sender, EventArgs e)
    {
        var values = new Dictionary<string, string> {
            { "emailAddress", EmailAddress },
            { "password", Password },
        };

        (int _statusCode, var response) = await _authenticate.newSession(values);

        NewSessionResponse jsonResponse = JsonSerializer.Deserialize<NewSessionResponse>(response);

        if (jsonResponse.user != null)
        {
            if (jsonResponse.user.emailAddressValidatedAt != null)
            {
                await _authenticate.setToken(jsonResponse.token);

                await _authenticate.setCurrentUser(jsonResponse.user);

                await Shell.Current.GoToAsync("accountpage");
            }

            signInErrors.Text = "";
            if (jsonResponse.user.errors.Length > 0)
            {
                foreach (string error in jsonResponse.user.errors)
                {
                    signInErrors.Text += error;
                    signInErrors.Text += "\n";
                }
            }

            if (jsonResponse.message != null)
            {
                ToastNotificationManagerCompat.History.Clear();

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

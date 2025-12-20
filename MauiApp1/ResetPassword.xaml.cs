using Microsoft.Toolkit.Uwp.Notifications;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Windows.Networking;


namespace MauiApp1;

public class ResetPasswordResponse
{

    public User user { get; set; }

    public string message { get; set; }

    public string sessionId { get; set; }

}

public partial class ResetPasswordPage : ContentPage
{

    private readonly IAuthenticate _authenticate;

    private string EmailAddress;

    private readonly AppShellViewModel _appShellViewModel;

    public ResetPasswordPage(IAuthenticate authenticate, AppShellViewModel appShellViewModel)
    {
        _authenticate = authenticate;
        _appShellViewModel = appShellViewModel;
        InitializeComponent();
        BindingContext = this;

        signInLink.Clicked += new EventHandler(signInLinkClicked);
    }

    private async void OnEmailAddressCompleted(object sender, EventArgs e)
    {
        EmailAddress = ((Entry)sender).Text;
    }

    public void signInLinkClicked(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync("signinpage");
    }

    private async void OnResetPasswordClicked(object sender, EventArgs e)
    {
        var values = new Dictionary<string, string> {
            { "emailAddress", EmailAddress },
        };

        (int _statusCode, var response) = await _authenticate.resetPassword(values);

        ResetPasswordResponse jsonResponse = JsonSerializer.Deserialize<ResetPasswordResponse>(response);

        ToastNotificationManagerCompat.History.Clear();

        if (_statusCode == 422)
        {
            resetPasswordErrors.Text = "User not found";
        } else
        {
            resetPasswordErrors.Text = "";
        }

        if (jsonResponse.message != null)
        {
            new ToastContentBuilder()
                .AddText(string.Concat(jsonResponse.message))
                .Show();
        }
    }
}

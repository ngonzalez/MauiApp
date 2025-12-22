using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Xaml;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Windows.Networking;


namespace MauiApp1;

public class ResetPasswordResponse
{

    public User user { get; set; }

    public string message { get; set; }

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

        switch(_statusCode)
        {
            case 200: resetPasswordInput.Text = ""; break;
            case 422: resetPasswordErrors.Text = "Email address not found"; break;
        }

        if (jsonResponse.message != null)
        {
            resetPasswordErrors.Text = jsonResponse.message;
        }
    }
}

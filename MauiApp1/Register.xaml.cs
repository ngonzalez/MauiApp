using Microsoft.Toolkit.Uwp.Notifications;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Windows.Networking;


namespace MauiApp1;

public class RegisterResponse
{

    public User user { get; set; }

    public string message { get; set; }

    public string sessionId { get; set; }

}

public partial class RegisterPage : ContentPage
{

    private readonly IAuthenticate _authenticate;

    private string FirstName;

    private string LastName;

    private string EmailAddress;

    private readonly AppShellViewModel _appShellViewModel;

    public RegisterPage(IAuthenticate authenticate, AppShellViewModel appShellViewModel)
    {
        _authenticate = authenticate;
        _appShellViewModel = appShellViewModel;
        InitializeComponent();
        BindingContext = this;

        signInLink.Clicked += new EventHandler(signInLinkClicked);
    }

    private async void OnFirstNameCompleted(object sender, EventArgs e)
    {
        FirstName = ((Entry)sender).Text;
    }

    private async void OnLastNameCompleted(object sender, EventArgs e)
    {
        LastName = ((Entry)sender).Text;
    }

    private async void OnEmailAddressCompleted(object sender, EventArgs e)
    {
        EmailAddress = ((Entry)sender).Text;
    }

    public void signInLinkClicked(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync("signinpage");
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var values = new Dictionary<string, string> {
            { "firstName", FirstName },
            { "lastName", LastName },
            { "emailAddress", EmailAddress },
        };

        (int _statusCode, var response) = await _authenticate.registerAccount(values);

        RegisterResponse jsonResponse = JsonSerializer.Deserialize<RegisterResponse>(response);

        ToastNotificationManagerCompat.History.Clear();

        if (jsonResponse.user != null)
        {
            if (jsonResponse.user.id != null)
            {
                new ToastContentBuilder()
                    .AddText(string.Concat(jsonResponse.message))
                    .Show();

            }

            registerErrors.Text = "";
            if (jsonResponse.user.errors.Length > 0)
            {
                foreach (string error in jsonResponse.user.errors)
                {
                    registerErrors.Text += error;
                    registerErrors.Text += "\n";
                }
            }

            if (jsonResponse.message != null)
            {
                new ToastContentBuilder()
                    .AddText(string.Concat(jsonResponse.message))
                    .Show();
            }
        }
    }
}

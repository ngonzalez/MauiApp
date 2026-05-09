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
}

public class AccountCodeResponse
{

    public Account account { get; set; }

    public string message { get; set; }
}

public partial class RegisterPage : ContentPage
{

    private readonly IAuthenticate _authenticate;

    private string AccountCode;

    private string AccountUUID;

    private string FirstName;

    private string LastName;

    private string EmailAddress;

    private string Password;

    private readonly AppShellViewModel _appShellViewModel;

    public RegisterPage(IAuthenticate authenticate, AppShellViewModel appShellViewModel)
    {
        _authenticate = authenticate;
        _appShellViewModel = appShellViewModel;
        InitializeComponent();
        BindingContext = this;

        signInLink.Clicked += new EventHandler(signInLinkClicked);
        AccountInfo.IsVisible = false;
    }

    public void signInLinkClicked(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync("signinpage");
    }

    private async void OnAccountCodeCompleted(object sender, EventArgs e)
    {
        AccountCode = ((Entry)sender).Text;

        var values = new Dictionary<string, string> {
            { "accountCode", AccountCode }
        };

        (int _statusCode, var response) = await _authenticate.sendAccountCode(values);

        AccountCodeResponse jsonResponse = JsonSerializer.Deserialize<AccountCodeResponse>(response);

        if (jsonResponse.account != null)
        {
            Account account = jsonResponse.account;
            AccountUUID = Convert.ToString(account.uuid);
            accountUuid.Text = Convert.ToString(account.uuid);
            accountName.Text = account.name;
            accountAddress.Text = account.address;
            AccountInfo.IsVisible = true;
        }
        else
        {
            AccountUUID = "";
            accountUuid.Text = "";
            accountName.Text = "";
            accountAddress.Text = "";
            AccountInfo.IsVisible = false;
        }
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

    private async void OnPasswordCompleted(object sender, EventArgs e)
    {
        Password = ((Entry)sender).Text;
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        Guid uuid = Guid.NewGuid();
        var values = new Dictionary<string, string> {
            { "accountUuid", AccountUUID },
            { "uuid", Convert.ToString(uuid) },
            { "firstName", FirstName },
            { "lastName", LastName },
            { "emailAddress", EmailAddress },
            { "password", Password }
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

using Microsoft.Toolkit.Uwp.Notifications;
using System.Drawing;
using System.Text.Json;

namespace MauiApp1;

public class DeleteSessionResponse
{

    public string message { get; set; }

}

public partial class AccountPage : ContentPage
{
    private readonly IAuthenticate _authenticate;

    private readonly AppShellViewModel _appShellViewModel;

    public AccountPage(IAuthenticate authenticate, AppShellViewModel appShellViewModel)
    {
        _authenticate = authenticate;
        _appShellViewModel = appShellViewModel;

        var sessionID = _appShellViewModel.SessionID;

        if (sessionID == null || sessionID == 0)
        {
            Shell.Current.GoToAsync("signinpage");
            return;
        }

        InitializeComponent();

        myAccountLabel.Text = "My Account (" + _appShellViewModel.CurrentUser.emailAddress + ")";
        uploadsLink.Clicked += new EventHandler(uploadsLinkClicked);
        signOutLink.Clicked += new EventHandler(signOutLinkClicked);
        accountLink.Clicked += new EventHandler(accountLinkClicked);
    }

    public async void signOutLinkClicked(object sender, EventArgs e)
    {
        (int _statusCode, var response) = await _authenticate.deleteSession();

        DeleteSessionResponse jsonResponse = JsonSerializer.Deserialize<DeleteSessionResponse>(response);

        //await DisplayAlert("Login", string.Concat(_jsonResponse.message), "OK");
        ToastNotificationManagerCompat.History.Clear();

        new ToastContentBuilder()
            .AddText(string.Concat(jsonResponse.message))
            .Show();

        _authenticate.setCurrentUser(new User { });

        _authenticate.setSessionID(0);

        Shell.Current.GoToAsync("signinpage");
    }

    public void accountLinkClicked(object sender, EventArgs e)
    {
        if (_appShellViewModel.CurrentUser.id == null)
        {
            Shell.Current.GoToAsync("signinpage");
        }
        else
        {
            Shell.Current.GoToAsync("mainaccountpage");
        }
    }

    public void uploadsLinkClicked(object sender, EventArgs e)
    {
        if (_appShellViewModel.CurrentUser.id == null)
        {
            Shell.Current.GoToAsync("signinpage");
        }
        else
        {
            Shell.Current.GoToAsync("mainpage");
        }
    }
}

using Microsoft.Toolkit.Uwp.Notifications;
using System.Drawing;
using System.Text.Json;
using MauiApp1.Resources.Strings;

namespace MauiApp1;

public class DeleteSessionResponse
{

    public string message { get; set; }

}

public partial class AccountPage : ContentPage
{
    private readonly IAuthenticate _authenticate;

    private readonly AppShellViewModel _appShellViewModel;

    private readonly IApiService _apiService;

    public AccountPage(IAuthenticate authenticate, AppShellViewModel appShellViewModel, IApiService apiService)
    {
        _authenticate = authenticate;
        _appShellViewModel = appShellViewModel;
        _apiService = apiService;

        if (string.IsNullOrEmpty(_appShellViewModel.Token))
        {
            Shell.Current.GoToAsync("signinpage");
            return;
        }

        InitializeComponent();

        myAccountLabel.Text = AppResources.Format("MyAccountWithEmail", _appShellViewModel.CurrentUser.emailAddress);
        uploadsLink.Clicked += new EventHandler(uploadsLinkClicked);
        listFoldersList.Clicked += new EventHandler(listFoldersListClicked);
        signOutLink.Clicked += new EventHandler(signOutLinkClicked);
        accountLink.Clicked += new EventHandler(accountLinkClicked);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RefreshPlanUsage();
    }

    // Below the email address: the account's plan, its storage and its users,
    // read again each time the page appears; hidden when it can't be read
    private async Task RefreshPlanUsage()
    {
        // without a sign-in the page goes to the sign-in, its controls aren't built
        if (planPanel == null || string.IsNullOrEmpty(_appShellViewModel.Token))
        {
            return;
        }
        PlanUsage? plan = await _apiService.GetPlanUsageAsync();
        planPanel.IsVisible = plan != null;
        if (plan == null)
        {
            return;
        }
        planNameLabel.Text = AppResources.Format("PlanName", plan.Name);
        planStorageLabel.Text = AppResources.Format("PlanStorage", PlanUsage.FormatSize(plan.StorageUsed), PlanUsage.FormatSize(plan.StorageLimit));
        planStorageBar.Progress = plan.StorageFraction;
        planStorageBar.ProgressColor = plan.StorageUsed >= plan.StorageLimit ? Colors.Red : Colors.CornflowerBlue;
        planUsersLabel.Text = AppResources.Format("PlanUsers", plan.UsersCount, plan.UsersLimit);
        planUsersBar.Progress = plan.UsersLimit > 0 ? Math.Min((double)plan.UsersCount / plan.UsersLimit, 1) : 0;
        planUsersBar.ProgressColor = plan.UsersCount >= plan.UsersLimit ? Colors.Red : Colors.CornflowerBlue;
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

        _authenticate.setToken(null);

        Shell.Current.GoToAsync("signinpage");
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

    public async void listFoldersListClicked(object sender, EventArgs e)
    {
        if (_appShellViewModel.CurrentUser.id == null)
        {
            Shell.Current.GoToAsync("signinpage");
        }
        else
        {
            Shell.Current.GoToAsync("folderlistpage");
        }

    }

    public void accountLinkClicked(object sender, EventArgs e)
    {
        if (_appShellViewModel.CurrentUser.id == null)
        {
            Shell.Current.GoToAsync("signinpage");
        }
        else
        {
            Shell.Current.GoToAsync("editaccountpage");
        }
    }
}

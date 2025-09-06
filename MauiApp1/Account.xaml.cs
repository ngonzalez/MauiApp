using MauiApp1.Platforms.Windows;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Net.Mail;
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
        InitializeComponent();
        myAccountLabel.Text = "My Account (" + _appShellViewModel.CurrentUser.emailAddress + ")";
        uploadsLink.Clicked += new EventHandler(uploadsLinkClicked);
        signOutLink.Clicked += new EventHandler(signOutLinkClicked);
        if (_appShellViewModel.SessionID == 0)
        {
            Shell.Current.GoToAsync("signin");
        }
    }

    public async void signOutLinkClicked(object sender, EventArgs e)
    {
        var response = await _authenticate.deleteSession();

        DeleteSessionResponse _jsonResponse = JsonSerializer.Deserialize<DeleteSessionResponse>(response);

        // await DisplayAlert("Login", string.Concat(jsonResponse.message), "OK");

        _authenticate.setCurrentUser(new User { });

        _authenticate.setSessionID(0);

        Shell.Current.GoToAsync("signin");
    }
    public void uploadsLinkClicked(object sender, EventArgs e)
    {
        if (_appShellViewModel.CurrentUser.id == null)
        {
            Shell.Current.GoToAsync("signin");
        } else
        {
            Shell.Current.GoToAsync("mainpage");
        }
    }
}

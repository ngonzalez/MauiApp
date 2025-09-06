using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text.Json;

namespace MauiApp1;
public partial class AccountPage : ContentPage
{
    private readonly AppShellViewModel _appShellViewModel;
    public AccountPage(AppShellViewModel appShellViewModel)
	{
        _appShellViewModel = appShellViewModel;
        InitializeComponent();
        myAccountLabel.Text = "My Account (" + _appShellViewModel.CurrentUser.emailAddress + ")";
        uploadsLink.Clicked += new EventHandler(uploadsLinkClicked);
        _appShellViewModel.HomeIsVisible = true;
    }
    public void uploadsLinkClicked(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync("mainpage");
    }
}

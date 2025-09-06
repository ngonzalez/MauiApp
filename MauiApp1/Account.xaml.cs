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
        uploadsLink.Clicked  += new EventHandler(uploadsLinkClicked);
        myAccountLabel.Text = "My Account (" + _appShellViewModel.CurrentUser.emailAddress + ")";
    }
    public void uploadsLinkClicked(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync("mainpage");
    }
}

using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MauiApp1;
public class NewSessionResponse
{
    public User user { get; set; }
    public string message { get; set; }
    public int sessionId { get; set; }
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
    }
    private async void OnSignInClicked(object sender, EventArgs e)
    {
        var values = new Dictionary<string, string> {
            { "emailAddress", EmailAddress },
            { "password", Password },
        };

        var response = await _authenticate.newSession(values);
        var jsonResponse = JsonSerializer.Deserialize<NewSessionResponse>(response);

        if (jsonResponse?.sessionId != null)
        {
            _authenticate.setSessionID(jsonResponse.sessionId);
        }

        if (jsonResponse?.user != null)
        {
            if (jsonResponse?.user?.id != null)
            {
                await _authenticate.setCurrentUser(jsonResponse.user);
                await Shell.Current.GoToAsync("mainpage");
            }
            else
            {
                await DisplayAlert("Login", string.Concat(jsonResponse.user.errors), "OK");
            }
        } else
        {
            await DisplayAlert("Login", string.Concat(jsonResponse.message), "OK");
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

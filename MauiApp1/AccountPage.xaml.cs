using System.Net.Http.Json;
using System.Text.Json;

namespace MauiApp1;
public class NewSessionResponse
{
    public UserResponse user { get; set; }
    public string[] errors { get; set; }
    public string message { get; set; }
}

public class UserResponse
{
    public Guid? uuid { get; set; }
    public string? firstName { get; set; }
    public string? lastName { get; set; }
    public string? emailAddress { get; set; }
    public bool deliverNotificationsSignIn { get; set; }
    public bool deliverNotificationsAccountUpdate { get; set; }
}

public partial class AccountPage : ContentPage
{
    private readonly IAuthenticate _authenticate;

    private string EmailAddress;

    private string Password;
    public AccountPage(IAuthenticate authenticate)
	{
        _authenticate = authenticate;
        InitializeComponent();
    }
    private async void OnSignInClicked(object sender, EventArgs e)
    {
        var values = new Dictionary<string, string> {
            { "email_address", EmailAddress },
            { "password", Password },
        };

        var response = await _authenticate.newSession(values);
        var jsonResponse = JsonSerializer.Deserialize<NewSessionResponse>(response);
        await DisplayAlert("Login", jsonResponse?.message, "OK");
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

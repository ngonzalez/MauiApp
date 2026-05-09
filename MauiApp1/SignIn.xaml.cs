using Microsoft.Toolkit.Uwp.Notifications;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;


namespace MauiApp1;

public class NewSessionResponse
{

    public User user { get; set; }

    public string message { get; set; }

    public string sessionId { get; set; }

}

public class SessionInfo
{
    public int sessionId { get; set; }

    public DateTime expiresAt { get; set; }

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
        BindingContext = this;
    }

    public async void OnResetPasswordClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("resetpasswordpage");
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("registerpage");
    }

    private async void OnSignInClicked(object sender, EventArgs e)
    {
        var values = new Dictionary<string, string> {
            { "emailAddress", EmailAddress },
            { "password", Password },
        };

        (int _statusCode, var response) = await _authenticate.newSession(values);

        NewSessionResponse jsonResponse = JsonSerializer.Deserialize<NewSessionResponse>(response);

        if (jsonResponse.user != null)
        {
            if (jsonResponse.user.emailAddressValidatedAt != null)
            {
                string sessionInfo = getSessionId(jsonResponse);

                SessionInfo jsonSessionInfo = JsonSerializer.Deserialize<SessionInfo>(sessionInfo);

                int sessionId = jsonSessionInfo.sessionId;

                await _authenticate.setSessionID(sessionId);

                await _authenticate.setCurrentUser(jsonResponse.user);

                await Shell.Current.GoToAsync("accountpage");
            }

            signInErrors.Text = "";
            if (jsonResponse.user.errors.Length > 0)
            {
                foreach (string error in jsonResponse.user.errors)
                {
                    signInErrors.Text += error;
                    signInErrors.Text += "\n";
                }
            }

            if (jsonResponse.message != null)
            {
                ToastNotificationManagerCompat.History.Clear();

                new ToastContentBuilder()
                    .AddText(string.Concat(jsonResponse.message))
                    .Show();
            }
        }
    }

    private string getSessionId(NewSessionResponse jsonResponse)
    {
        // session id
        string sessionId = jsonResponse.sessionId;
        string[] subs = sessionId.Split(':');

        // iv
        string iv_hex = subs[0];
        byte[] iv_data = Enumerable.Range(0, iv_hex.Length)
                                   .Where(x => x % 2 == 0)
                                   .Select(x => Convert.ToByte(iv_hex.Substring(x, 2), 16))
                                   .ToArray();

        // encrypted
        string encrypted_hex = subs[1];
        byte[] encrypted_data = Enumerable.Range(0, encrypted_hex.Length)
                                          .Where(x => x % 2 == 0)
                                          .Select(x => Convert.ToByte(encrypted_hex.Substring(x, 2), 16))
                                          .ToArray();

        // aes
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes("a797255fd895fd168cf4b44057a99da2"); // SECRET_KEY_BASE[0, 32]
        aes.IV = iv_data;
        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(encrypted_data);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        return sr.ReadToEnd();
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

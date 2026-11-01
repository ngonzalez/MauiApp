using MauiApp1.Resources.Strings;
using MauiApp1.Platforms.Windows;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO.Compression;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MauiApp1
{

    public class UpdateAccountResponse
    {

        public User user { get; set; }

        public string message { get; set; }

    }

    public class UpdatePasswordResponse
    {

        public User user { get; set; }

        public string message { get; set; }

    }

    public class UpdateEmailAddressResponse
    {

        public User user { get; set; }

        public string message { get; set; }

    }

    public partial class EditAccountPage : ContentPage
    {
        private readonly IAuthenticate _authenticate;

        private readonly AppShellViewModel _appShellViewModel;

        private readonly IApiService _apiService;

        private string FirstName;

        private string LastName;

        private string EmailAddress;

        private string Password;

        private string PasswordConfirmation;

        public EditAccountPage(IAuthenticate authenticate, AppShellViewModel appShellViewModel, IApiService apiService)
        {
            _authenticate = authenticate;

            _appShellViewModel = appShellViewModel;

            _apiService = apiService;

            if (string.IsNullOrEmpty(_appShellViewModel.Token))
            {
                Shell.Current.GoToAsync("signinpage");
            }

            InitializeComponent();

            BindingContext = this;

            myAccountLink.Clicked += new EventHandler(accountLinkClicked);
            accountUuid.Text = Convert.ToString(_appShellViewModel.CurrentUser.accountUuid);
            accountName.Text = _appShellViewModel.CurrentUser.accountName;
            accountSubdomain.Text = _appShellViewModel.CurrentUser.accountSubdomain;
            accountAddress.Text = _appShellViewModel.CurrentUser.accountAddress;
            editAccountFirstName.Text = _appShellViewModel.CurrentUser.firstName;
            editAccountLastName.Text = _appShellViewModel.CurrentUser.lastName;
            editAccountEmailAddress.Text = _appShellViewModel.CurrentUser.emailAddress;
            editAccountCreatedAt.Text = Convert.ToString(_appShellViewModel.CurrentUser.createdAt);
            editAccountUpdatedAt.Text = Convert.ToString(_appShellViewModel.CurrentUser.updatedAt);
            changeEmailCurrentEmailAddress.Text = _appShellViewModel.CurrentUser.emailAddress;
            editAccountDeliverNotificationsOnSignIn.IsChecked = _appShellViewModel.CurrentUser.deliverNotificationsSignIn;
            editAccountDeliverNotificationsOnAccountUpdate.IsChecked = _appShellViewModel.CurrentUser.deliverNotificationsAccountUpdate!;
        }

        public void accountLinkClicked(object sender, EventArgs e)
        {
            Shell.Current.GoToAsync("accountpage");
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

        public async void OnUpdateAccountClicked(object sender, EventArgs e)
        {
            var values = new Dictionary<string, string> {
                { "id", Convert.ToString(_appShellViewModel.CurrentUser.id!) },
                { "firstName", FirstName },
                { "lastName", LastName },
                { "emailAddress", EmailAddress },
                { "deliverNotificationsSignIn", Convert.ToString(editAccountDeliverNotificationsOnSignIn.IsChecked).ToLower() },
                { "deliverNotificationsAccountUpdate", Convert.ToString(editAccountDeliverNotificationsOnAccountUpdate.IsChecked).ToLower() },
            };

            (int statusCode, var response) = await _authenticate.updateAccount(values);

            UpdateAccountResponse jsonResponse = JsonSerializer.Deserialize<UpdateAccountResponse>(response);

            accountErrors.Text = "";
            if (jsonResponse.user.errors.Length > 0)
            {
                foreach (string error in jsonResponse.user.errors)
                {
                    accountErrors.Text += error;
                    accountErrors.Text += "\n";
                }
            }
            // refused: nothing was saved
            if (statusCode == 422 && jsonResponse?.message != null)
            {
                accountErrors.Text += jsonResponse.message;
            }

            ToastNotificationManagerCompat.History.Clear();

            if (jsonResponse?.message != null)
            {
                _authenticate.setCurrentUser(jsonResponse.user);

                new ToastContentBuilder()
                    .AddText(string.Concat(jsonResponse.message))
                    .Show();
            }
        }

        private async void OnPasswordCompleted(object sender, EventArgs e)
        {
            Password = ((Entry)sender).Text;
        }

        private async void OnPasswordConfirmationCompleted(object sender, EventArgs e)
        {
            PasswordConfirmation = ((Entry)sender).Text;
        }

        public async void OnUpdatePasswordClicked(object sender, EventArgs e)
        {
            var problems = PasswordRules.Check(Password, _appShellViewModel.CurrentUser.emailAddress);
            if (problems.Count > 0)
            {
                passwordErrors.Text = string.Join("\n", problems.Select(problem => AppResources.Get(PasswordRules.MessageKey(problem))));
                return;
            }

            var values = new Dictionary<string, string> {
                { "id", Convert.ToString(_appShellViewModel.CurrentUser.id!) },
                { "password", Password },
                { "passwordConfirmation", PasswordConfirmation }
            };

            (int statusCode, var response) = await _authenticate.updatePassword(values);
            
            UpdatePasswordResponse jsonResponse = JsonSerializer.Deserialize<UpdatePasswordResponse>(response);
            
            passwordErrors.Text = "";
            if (jsonResponse.user.errors.Length > 0)
            {
                foreach (string error in jsonResponse.user.errors)
                {
                    passwordErrors.Text += error;
                    passwordErrors.Text += "\n";
                }
            }
            
            ToastNotificationManagerCompat.History.Clear();
            
            if (jsonResponse?.message != null)
            {
                if (statusCode == 200)
                {
                    await _authenticate.setCurrentUser(jsonResponse.user);

                    changePasswordPassword.Text = "";
                    changePasswordPasswordConfirmation.Text = "";
                }

                new ToastContentBuilder()
                    .AddText(string.Concat(jsonResponse.message))
                    .Show();
            }
        }

        public void OnNewEmailAddressCompleted(object sender, EventArgs e)
        {
            EmailAddress = ((Entry)sender).Text;
        }
        public async void OnUpdateEmailClicked(object sender, EventArgs e)
        {
            var values = new Dictionary<string, string> {
                { "id", Convert.ToString(_appShellViewModel.CurrentUser.id!) },
                { "emailAddress", EmailAddress }
            };

            (int statusCode, var response) = await _authenticate.updateEmailAddress(values);

            UpdateEmailAddressResponse jsonResponse = JsonSerializer.Deserialize<UpdateEmailAddressResponse>(response);

            emailAddressErrors.Text = "";
            if (jsonResponse.user.errors.Length > 0)
            {
                foreach (string error in jsonResponse.user.errors)
                {
                    emailAddressErrors.Text += error;
                    emailAddressErrors.Text += "\n";
                }
            }

            ToastNotificationManagerCompat.History.Clear();

            if (jsonResponse?.message != null)
            {
                if (statusCode == 200)
                {
                    await _authenticate.setCurrentUser(jsonResponse.user);

                    changeEmailNewEmailAddress.Text = "";
                    changeEmailNewEmailAddress.Text = "";
                }

                new ToastContentBuilder()
                    .AddText(string.Concat(jsonResponse.message))
                    .Show();
            }
        }

        // True while an invitation is sent: Enter then a click doesn't send it twice
        private bool _sendingInvitation;

        // The Send invitation button, or Enter in the address. The address is
        // checked first (InvitationClient.Check); a failure (network) is shown
        // under the form instead of closing the app
        public async void OnSendInvitationClicked(object sender, EventArgs e)
        {
            if (_sendingInvitation)
            {
                return;
            }

            string emailAddress = (invitationEmailAddress.Text ?? "").Trim();
            InvitationClient.Problem? problem = InvitationClient.Check(emailAddress);
            if (problem != null)
            {
                invitationErrors.Text = AppResources.Get(InvitationClient.MessageKey(problem.Value));
                return;
            }

            _sendingInvitation = true;
            invitationErrors.Text = "";
            try
            {
                InvitationClient.Result result = await _apiService.SendInvitationAsync(emailAddress);

                switch (result.Outcome)
                {
                    case InvitationClient.Outcome.Sent:
                        string message = result.Message ?? AppResources.Format("InvitationSentTo", emailAddress);
                        invitationEmailAddress.Text = "";
                        invitationErrors.Text = message;

                        ToastNotificationManagerCompat.History.Clear();

                        new ToastContentBuilder()
                            .AddText(message)
                            .Show();
                        break;

                    case InvitationClient.Outcome.NotSignedIn:
                        // the session is over: sign in again, like Sign out
                        await _authenticate.setCurrentUser(new User { });
                        await _authenticate.setToken(null);
                        await Shell.Current.GoToAsync("signinpage");
                        break;

                    case InvitationClient.Outcome.TooManyRequests:
                        invitationErrors.Text = AppResources.Get("InvitationTooMany");
                        break;

                    default:
                        invitationErrors.Text = result.Messages.Count > 0
                            ? string.Join("\n", result.Messages)
                            : AppResources.Format("InvitationFailed", result.StatusCode);
                        break;
                }
            }
            catch (Exception exception)
            {
                CrashLog.Write(exception, "SendInvitation");
                invitationErrors.Text = exception.Message;
            }
            finally
            {
                _sendingInvitation = false;
            }
        }
    }
}

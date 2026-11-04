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

    // My Account's forms, one at a time
    public enum AccountForm
    {
        EditAccount,
        ChangePassword,
        ChangeEmailAddress,
        InviteUsers
    }

    // The right pane of My Account (AccountPage): the form one of its buttons
    // opened. Opening another form, or Close, empties the one shown
    public partial class AccountForms : ContentView
    {
        private readonly IAuthenticate _authenticate;

        private readonly AppShellViewModel _appShellViewModel;

        private readonly IApiService _apiService;

        private string AccountName;
        private string AccountAddress;
        private string FirstName;

        private string LastName;

        private string EmailAddress;

        // Change Email Address's own field: Edit Account's EmailAddress is
        // another one
        private string NewEmailAddress;

        private string Password;

        private string PasswordConfirmation;

        // Built by AccountPage, which is only shown when signed in
        public AccountForms(IAuthenticate authenticate, AppShellViewModel appShellViewModel, IApiService apiService)
        {
            _authenticate = authenticate;

            _appShellViewModel = appShellViewModel;

            _apiService = apiService;

            InitializeComponent();

            BindingContext = this;

            FillFromCurrentUser();

            _ = LoadAvatarPresets();
        }

        // Show `form` alone, emptied
        public void Open(AccountForm form)
        {
            Reset();
            editAccountSection.IsVisible = form == AccountForm.EditAccount;
            changePasswordSection.IsVisible = form == AccountForm.ChangePassword;
            changeEmailSection.IsVisible = form == AccountForm.ChangeEmailAddress;
            inviteUsersSection.IsVisible = form == AccountForm.InviteUsers;
            IsVisible = true;
        }

        // Hide the form shown, emptied
        public void Close()
        {
            Reset();
            editAccountSection.IsVisible = false;
            changePasswordSection.IsVisible = false;
            changeEmailSection.IsVisible = false;
            inviteUsersSection.IsVisible = false;
            IsVisible = false;
        }

        public void OnCloseClicked(object sender, EventArgs e)
        {
            Close();
        }

        // What was typed and the messages go; the account's details are
        // read again from the signed-in user
        private void Reset()
        {
            changePasswordPassword.Text = "";
            changePasswordPasswordConfirmation.Text = "";
            changeEmailNewEmailAddress.Text = "";
            invitationEmailAddress.Text = "";

            avatarErrors.Text = "";
            accountErrors.Text = "";
            passwordErrors.Text = "";
            emailAddressErrors.Text = "";
            invitationErrors.Text = "";

            FillFromCurrentUser();
        }

        // The signed-in user's details in Edit Account and Change Email Address
        private void FillFromCurrentUser()
        {
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

        private async void OnAccountNameCompleted(object sender, EventArgs e)
        {
            AccountName = ((Entry)sender).Text;
        }

        private async void OnAccountAddressCompleted(object sender, EventArgs e)
        {
            AccountAddress = ((Entry)sender).Text;
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
                { "accountName", AccountName },
                { "accountAddress", AccountAddress },
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
            NewEmailAddress = ((Entry)sender).Text;
        }
        public async void OnUpdateEmailClicked(object sender, EventArgs e)
        {
            var values = new Dictionary<string, string> {
                { "id", Convert.ToString(_appShellViewModel.CurrentUser.id!) },
                { "emailAddress", NewEmailAddress }
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

        // True while the picture is sent or removed: a second click waits for it
        private bool _savingAvatar;

        // The presets under the picture; hidden when they can't be read (offline)
        private async Task LoadAvatarPresets()
        {
            try
            {
                (int statusCode, var response) = await _authenticate.getAvatarPresets();
                List<AvatarPreset> presets = statusCode == 200 ? AvatarRules.ParsePresets(response) : new List<AvatarPreset>();
                BindableLayout.SetItemsSource(avatarPresets, presets);
                avatarPresetsPanel.IsVisible = presets.Count > 0;
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"AvatarPresets: {exception.Message}");
                avatarPresetsPanel.IsVisible = false;
            }
        }

        // Choose picture: a PNG, JPEG, WebP or GIF file of 5 MB at most, sent at once
        public async void OnChoosePictureClicked(object sender, EventArgs e)
        {
            if (_savingAvatar)
            {
                return;
            }

            avatarErrors.Text = "";
            try
            {
                FileResult? file = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = AppResources.Get("ChoosePicture"),
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.WinUI, AvatarRules.Extensions }
                    })
                });
                if (file == null)
                {
                    // cancelled
                    return;
                }

                AvatarRules.Problem? problem = AvatarRules.Check(file.FileName, new FileInfo(file.FullPath).Length);
                if (problem != null)
                {
                    avatarErrors.Text = AppResources.Get(AvatarRules.MessageKey(problem.Value));
                    return;
                }

                _savingAvatar = true;
                byte[] picture = await File.ReadAllBytesAsync(file.FullPath);
                (int statusCode, var response) = await _authenticate.updateAvatar(picture, file.FileName, AvatarRules.ContentType(file.FileName)!);
                await ShowAvatarResponse(statusCode, response);
            }
            catch (Exception exception)
            {
                CrashLog.Write(exception, "UpdateAvatar");
                avatarErrors.Text = exception.Message;
            }
            finally
            {
                _savingAvatar = false;
            }
        }

        // A preset's thumbnail: it becomes the picture at once
        private async void OnAvatarPresetTapped(object? sender, TappedEventArgs e)
        {
            if (_savingAvatar || (sender as BindableObject)?.BindingContext is not AvatarPreset preset)
            {
                return;
            }

            _savingAvatar = true;
            avatarErrors.Text = "";
            try
            {
                (int statusCode, var response) = await _authenticate.setAvatarPreset(preset.name);
                await ShowAvatarResponse(statusCode, response);
            }
            catch (Exception exception)
            {
                CrashLog.Write(exception, "SetAvatarPreset");
                avatarErrors.Text = exception.Message;
            }
            finally
            {
                _savingAvatar = false;
            }
        }

        public async void OnRemovePictureClicked(object sender, EventArgs e)
        {
            if (_savingAvatar)
            {
                return;
            }

            _savingAvatar = true;
            avatarErrors.Text = "";
            try
            {
                (int statusCode, var response) = await _authenticate.deleteAvatar();
                await ShowAvatarResponse(statusCode, response);
            }
            catch (Exception exception)
            {
                CrashLog.Write(exception, "DeleteAvatar");
                avatarErrors.Text = exception.Message;
            }
            finally
            {
                _savingAvatar = false;
            }
        }

        // The response of PUT or DELETE /user/avatar, like the account update's:
        // the errors under the picture, the message in a toast, the new user
        // (and so every avatar, CurrentUserAvatar) once it is saved
        private async Task ShowAvatarResponse(int statusCode, string response)
        {
            UpdateAccountResponse? jsonResponse = null;
            try
            {
                jsonResponse = JsonSerializer.Deserialize<UpdateAccountResponse>(response);
            }
            catch (JsonException)
            {
                // not the backend's JSON (a proxy's error page): the status below
            }

            if (jsonResponse?.message == null)
            {
                avatarErrors.Text = AppResources.Format("AvatarFailed", statusCode);
                return;
            }

            avatarErrors.Text = "";
            if (jsonResponse.user?.errors?.Length > 0)
            {
                avatarErrors.Text = string.Join("\n", jsonResponse.user.errors) + "\n";
            }
            // refused: nothing was saved
            if (statusCode != 200)
            {
                avatarErrors.Text += jsonResponse.message;
            }
            else if (jsonResponse.user != null)
            {
                await _authenticate.setCurrentUser(jsonResponse.user);
            }

            ToastNotificationManagerCompat.History.Clear();

            new ToastContentBuilder()
                .AddText(jsonResponse.message)
                .Show();
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

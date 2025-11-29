using MauiApp1.Platforms.Windows;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO.Compression;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text.Json;

namespace MauiApp1
{
    public class UpdateAccountResponse
    {

        public User user { get; set; }

        public string message { get; set; }

    }

    public partial class MainAccountPage : ContentPage
    {
        private readonly IAuthenticate _authenticate;

        private readonly AppShellViewModel _appShellViewModel;

        private string FirstName;

        private string LastName;

        public MainAccountPage(IAuthenticate authenticate, AppShellViewModel appShellViewModel)
        {
            _authenticate = authenticate;

            _appShellViewModel = appShellViewModel;

            var sessionID = _appShellViewModel.SessionID;

            if (sessionID == null || sessionID == 0)
            {
                Shell.Current.GoToAsync("signin");
            }

            InitializeComponent();

            BindingContext = this;

            myAccountLink.Clicked += new EventHandler(accountLinkClicked);
            editAccountFirstName.Text = _appShellViewModel.CurrentUser.firstName;
            editAccountLastName.Text = _appShellViewModel.CurrentUser.lastName;
            editAccountEmailAddress.Text = _appShellViewModel.CurrentUser.emailAddress;
            editAccountUuid.Text = Convert.ToString(_appShellViewModel.CurrentUser.uuid);
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

        public async void OnUpdateAccountClicked(object sender, EventArgs e)
        {
            var values = new Dictionary<string, string> {
                { "id", Convert.ToString(_appShellViewModel.CurrentUser.id!) },
                { "firstName", FirstName },
                { "lastName", LastName },
                { "emailAddress", _appShellViewModel.CurrentUser.emailAddress! },
                { "deliverNotificationsSignIn", Convert.ToString(editAccountDeliverNotificationsOnSignIn.IsChecked).ToLower() },
                { "deliverNotificationsAccountUpdate", Convert.ToString(editAccountDeliverNotificationsOnAccountUpdate.IsChecked).ToLower() }
            };

            var response = await _authenticate.updateAccount(values);

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

            updateAccountMessage.Text = "";
            if (jsonResponse?.message != null)
            {
                _authenticate.setCurrentUser(jsonResponse.user);

                updateAccountMessage.Text = jsonResponse.message;
            }
        }

        public void OnUpdatePasswordClicked(object sender, EventArgs e)
        {

        }

        public void OnUpdateEmailClicked(object sender, EventArgs e)
        {

        }
    }
}

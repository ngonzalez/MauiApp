using System.Collections.ObjectModel;
using System.Drawing;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

namespace MauiApp1
{
    public partial class MainAccountPage : ContentPage
    {

        private readonly IApiService _apiService;

        private readonly AppShellViewModel _appShellViewModel;

        public MainAccountPage(IApiService apiService, AppShellViewModel appShellViewModel)
        {
            _apiService = apiService;
            _appShellViewModel = appShellViewModel;

            var sessionID = _appShellViewModel.SessionID;

            if (sessionID == null || sessionID == 0)
            {
                Shell.Current.GoToAsync("signin");
            }

            InitializeComponent();

            BindingContext = this;

            myAccountLink.Clicked += new EventHandler(accountLinkClicked);
        }

        public void accountLinkClicked(object sender, EventArgs e)
        {
            Shell.Current.GoToAsync("accountpage");
        }
    }
}

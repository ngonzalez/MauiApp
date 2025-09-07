using MauiApp1.Platforms.Windows;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;
using Windows.Security.Cryptography.Core;

namespace MauiApp1
{
    public partial class AppShell : Shell
    {
        private bool IsVisible { get; set; }
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("signin", typeof(SignInPage));
            Routing.RegisterRoute("account", typeof(AccountPage));
            Routing.RegisterRoute("mainpage", typeof(MainPage));

            BindingContext = new AppShellViewModel();
        }

        protected override void OnNavigating(ShellNavigatingEventArgs args)
        {
            base.OnNavigating(args);

            // Cancel any back navigation.
            if (args.Source == ShellNavigationSource.Pop)
            {
                args.Cancel();
            }
        }
    }
}

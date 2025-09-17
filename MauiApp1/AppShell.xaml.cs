using MauiApp1.Platforms.Windows;
using System.Net.Mail;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using Windows.Security.Cryptography.Core;

namespace MauiApp1
{
    public partial class AppShell : Shell
    {

        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("signin", typeof(SignInPage));
            Routing.RegisterRoute("account", typeof(AccountPage));
            Routing.RegisterRoute("mainpage", typeof(MainPage));
            Routing.RegisterRoute("displayitempage", typeof(DisplayItemPage));

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

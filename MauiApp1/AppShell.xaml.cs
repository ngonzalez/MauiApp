using MauiApp1.Platforms.Windows;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;

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

            BindingContext = new AppShellViewModel();
        }
    }
}

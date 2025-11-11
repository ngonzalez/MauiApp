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
            Routing.RegisterRoute("displaypage", typeof(DisplayPage));
            Routing.RegisterRoute("showimagefilepage", typeof(ShowImageFilePage));

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

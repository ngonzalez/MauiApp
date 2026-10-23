namespace MauiApp1
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("signinpage", typeof(SignInPage));
            Routing.RegisterRoute("folderlistpage", typeof(FolderListPage));
            Routing.RegisterRoute("registerpage", typeof(RegisterPage));
            Routing.RegisterRoute("resetpasswordpage", typeof(ResetPasswordPage));
            Routing.RegisterRoute("accountpage", typeof(AccountPage));
            Routing.RegisterRoute("editaccountpage", typeof(EditAccountPage));
            Routing.RegisterRoute("mainpage", typeof(MainPage));
            Routing.RegisterRoute("displaypage", typeof(DisplayPage));
            Routing.RegisterRoute("showimagefilepage", typeof(ShowImageFilePage));

            BindingContext = new AppShellViewModel();
        }

        // The flyout's Account item: My Account when signed in (unless it is
        // already shown), the Sign In page otherwise. The token is in the
        // AppShellViewModel the pages share (MauiProgram), not in this
        // shell's BindingContext
        private async void AccountMenuItemClicked(object sender, EventArgs e)
        {
            var viewModel = IPlatformApplication.Current?.Services.GetService<AppShellViewModel>();

            if (string.IsNullOrEmpty(viewModel?.Token))
            {
                await GoToAsync("signinpage");
            }
            else if (CurrentPage is not AccountPage)
            {
                await GoToAsync("accountpage");
            }
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

namespace MauiApp1
{
    public partial class StatusBanner : ContentView
    {
        public StatusBanner()
        {
            InitializeComponent();
            BindingContext = StatusMonitor.Shared;
        }

        private async void DetailsTapped(object? sender, TappedEventArgs e)
        {
            await Launcher.OpenAsync(new Uri(StatusMonitor.PageUrl));
        }

        private void HideTapped(object? sender, TappedEventArgs e)
        {
            StatusMonitor.Shared.Dismiss();
        }
    }
}

namespace MauiApp1.Platforms.Windows
{
    // The alerts are shown by the page of the app's window
    // (Application.MainPage is deprecated since .NET MAUI 9)
    public class AlertService : IAlertService
    {
        private static Page CurrentPage()
        {
            return Application.Current?.Windows.FirstOrDefault()?.Page
                ?? throw new InvalidOperationException("The app has no window to show the alert in.");
        }

        public async Task DisplayAlertAsync(string title, string message, string accept)
        {
            await CurrentPage().DisplayAlert(title, message, accept);
        }

        public async Task<bool> DisplayAlertAsync(string title, string message, string accept, string cancel)
        {
            return await CurrentPage().DisplayAlert(title, message, accept, cancel);
        }
    }
}

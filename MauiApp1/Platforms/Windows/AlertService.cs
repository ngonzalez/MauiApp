namespace MauiApp1.Platforms.Windows
{
    public class AlertService : IAlertService
    {
        public async Task DisplayAlertAsync(string title, string message, string accept)
        {
            var mainPage = Application.Current?.MainPage;
            if (mainPage == null)
                throw new InvalidOperationException("MainPage is not set. Ensure the app has a MainPage.");

            await mainPage.DisplayAlert(title, message, accept);
        }

        public async Task<bool> DisplayAlertAsync(string title, string message, string accept, string cancel)
        {
            var mainPage = Application.Current?.MainPage;
            if (mainPage == null)
                throw new InvalidOperationException("MainPage is not set. Ensure the app has a MainPage.");

            return await mainPage.DisplayAlert(title, message, accept, cancel);
        }
    }
}

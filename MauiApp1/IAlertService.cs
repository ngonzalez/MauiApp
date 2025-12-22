namespace MauiApp1
{    public interface IAlertService
    {
        Task DisplayAlertAsync(string title, string message, string accept);

        Task<bool> DisplayAlertAsync(string title, string message, string accept, string cancel);
    }
}

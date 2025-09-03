namespace MauiApp1
{
    public interface IApiService
    {
        Task<string> CreatePostAsync(byte[] body);
    }
}

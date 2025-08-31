namespace MauiApp1
{
    public interface IApiService
    {
        Task<string> CreatePostAsync(System.Net.Http.HttpContent jsonContent);
    }
}

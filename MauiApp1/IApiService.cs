namespace MauiApp1
{
    public interface IApiService
    {
        Task<string> GetAllUploads(string ids);
        Task<string> CreatePostAsync(byte[] body);
    }
}

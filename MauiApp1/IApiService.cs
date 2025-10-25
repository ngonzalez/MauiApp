namespace MauiApp1
{
    public interface IApiService
    {
        Task<string> getVideoStream(string id);

        Task<string> GetAllUploads(string ids);

        Task<string> CreatePostAsync(byte[] body);
    }
}

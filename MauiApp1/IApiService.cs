namespace MauiApp1
{
    public interface IApiService
    {
        Task<(int, String)> getAudioStream(string id);

        Task<(int, String)> getVideoStream(string id);

        Task<(int, String)> GetAllUploads(string ids);

        Task<(int, String)> CreatePostAsync(byte[] body);
    }
}

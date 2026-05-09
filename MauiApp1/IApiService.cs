namespace MauiApp1
{
    public interface IApiService
    {
        Task<(int, String)> getUploads(Guid accountUuid, string ids);

        Task<(int, String)> CreatePostAsync(byte[] body);

        Task<(int, String)> getVideoStream(string id);

        Task<(int, String)> getAudioStream(string id);

        Task<(int, String)> PublishFolders(byte[] body);

        Task<(int, String)> UnpublishFolders(byte[] body);

        Task<(int, String)> ArchiveFolders(byte[] body);

        Task<(int, String)> UnarchiveFolders(byte[] body);

        Task<(int, String)> DeleteFolders(byte[] body);

        Task<(int, String)> DeleteAttachments(byte[] body);

        Task<(int, String)> CreateEvent(byte[] body);
    }
}

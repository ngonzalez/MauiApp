namespace MauiApp1
{
    public interface IApiService
    {
        Task<(int, String)> getUploads(Guid accountUuid, string ids);

        // Upload a file straight to the storage (DirectUploadClient); returns the upload's uuid
        Task<Guid> UploadFileAsync(UploadFile uploadFile);

        // The signed-in account's plan and what it uses of it (GET /account);
        // null when it can't be read
        Task<PlanUsage?> GetPlanUsageAsync();

        Task<(int, String)> getVideoStream(string id);

        Task<(int, String)> getAudioStream(string id);

        Task<(int, String)> PublishFolders(byte[] body);

        Task<(int, String)> UnpublishFolders(byte[] body);

        Task<(int, String)> ArchiveFolders(byte[] body);

        Task<(int, String)> UnarchiveFolders(byte[] body);

        Task<(int, String)> DeleteFolders(byte[] body);

        Task<(int, String)> DeleteAttachments(byte[] body);

        Task<(int, String)> CreateEvent(byte[] body);

        // Invite someone by email to join the signed-in user's account
        // (POST /invitations, InvitationClient); throws on a network failure
        Task<InvitationClient.Result> SendInvitationAsync(string emailAddress);
    }
}

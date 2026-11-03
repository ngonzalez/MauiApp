namespace MauiApp1
{
    public interface IAuthenticate
    {

        Task<User> getCurrentUser();

        Task<bool> setCurrentUser(User user);

        Task<bool> setToken(string? token);

        Task<string?> getToken();

        Task<(int, String)> newSession(Dictionary<string, string> values);

        Task<(int, String)> deleteSession();

        Task<(int, String)> updateAccount(Dictionary<string, string> values);

        Task<(int, String)> updatePassword(Dictionary<string, string> values);

        Task<(int, String)> resetPassword(Dictionary<string, string> values);

        Task<(int, String)> updateEmailAddress(Dictionary<string, string> values);

        // The account picture: a PNG, JPEG, WebP or GIF file (PUT /user/avatar)
        Task<(int, String)> updateAvatar(byte[] picture, string fileName, string contentType);

        // The account picture: one of the presets, by name (PUT /user/avatar)
        Task<(int, String)> setAvatarPreset(string name);

        Task<(int, String)> deleteAvatar();

        // The pictures to choose from (GET /avatars/presets, public)
        Task<(int, String)> getAvatarPresets();

    }
}

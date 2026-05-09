namespace MauiApp1
{
    public interface IAuthenticate
    {

        Task<User> getCurrentUser();

        Task<bool> setCurrentUser(User user);

        Task<bool> setSessionID(int sessionID);

        Task<int> getSessionID();

        Task<(int, String)> registerAccount(Dictionary<string, string> values);

        Task<(int, String)> sendAccountCode(Dictionary<string, string> values);

        Task<(int, String)> newSession(Dictionary<string, string> values);

        Task<(int, String)> deleteSession();

        Task<(int, String)> updateAccount(Dictionary<string, string> values);

        Task<(int, String)> updatePassword(Dictionary<string, string> values);

        Task<(int, String)> resetPassword(Dictionary<string, string> values);

        Task<(int, String)> updateEmailAddress(Dictionary<string, string> values);

    }
}

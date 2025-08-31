namespace MauiApp1
{
    public interface IAuthenticate
    {
        Task<User> getCurrentUser();
        Task<bool> setCurrentUser(User user);
        Task<String> newSession(Dictionary<string, string> values);
        Task<bool> setSessionID(int sessionID);
        Task<int> getSessionID();
    }
}

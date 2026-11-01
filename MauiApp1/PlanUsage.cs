using System.Globalization;
using System.Text.Json;

namespace MauiApp1
{
    // The account's plan and what it uses of it: the "plan" of the backend's
    // GET /account, with the signed-in session's token. Pro: files up to
    // 5 GiB and 3 users; Business: 10 GiB and 10 users. Every plan is paid:
    // without one (an empty name) the account's site is offline.
    public record PlanUsage(string Name, long StorageUsed, long StorageLimit, int UsersCount, int UsersLimit)
    {
        // How much of the storage the files take, from 0 to 1
        public double StorageFraction => StorageLimit > 0 ? Math.Min((double)StorageUsed / StorageLimit, 1) : 0;

        // False without a plan: no subscription, or it ended
        public bool HasPlan => !string.IsNullOrEmpty(Name);

        // True when the account's plan has no room left for files or users
        // (never without a plan: it has none to fill)
        public bool IsFull => HasPlan && (StorageUsed >= StorageLimit || UsersCount >= UsersLimit);

        // The plan of a GET /account answer; null without one (a backend
        // before plans) or for an answer that can't be read
        public static PlanUsage? Parse(string json)
        {
            try
            {
                using var document = JsonDocument.Parse(json);
                if (!document.RootElement.TryGetProperty("account", out JsonElement account) ||
                    !account.TryGetProperty("plan", out JsonElement plan) || plan.ValueKind != JsonValueKind.Object)
                {
                    return null;
                }
                JsonElement storage = plan.GetProperty("storage");
                JsonElement users = plan.GetProperty("users");
                return new PlanUsage(plan.GetProperty("name").GetString() ?? "",
                                     storage.GetProperty("used").GetInt64(), storage.GetProperty("limit").GetInt64(),
                                     users.GetProperty("count").GetInt32(), users.GetProperty("limit").GetInt32());
            }
            catch (Exception exception) when (exception is JsonException || exception is KeyNotFoundException || exception is InvalidOperationException || exception is FormatException)
            {
                return null;
            }
        }

        // A size in binary units, like the plans' names and the backend:
        // 5 GB for 5 GiB, 49 MB, 4.9 GB (in the culture's number format)
        public static string FormatSize(long bytes, CultureInfo? culture = null)
        {
            string[] units = { "bytes", "KB", "MB", "GB", "TB" };
            double value = bytes;
            int unit = 0;
            while (value >= 1024 && unit < units.Length - 1)
            {
                value /= 1024;
                unit++;
            }
            string format = unit == 0 || value >= 10 || Math.Abs(value - Math.Round(value)) < 0.05 ? "0" : "0.0";
            return value.ToString(format, culture ?? CultureInfo.CurrentCulture) + " " + units[unit];
        }
    }
}

using System.Text.Json;

namespace MauiApp1
{
    // A ready-made picture to choose instead of a file (GET /avatars/presets)
    public class AvatarPreset
    {
        public string name { get; set; } = "";
        public string url { get; set; } = "";
    }

    public class AvatarPresetsResponse
    {
        public List<AvatarPreset>? presets { get; set; }
    }

    // The account picture's rules (PUT /user/avatar): a PNG, JPEG, WebP or
    // GIF file of 5 MB at most, checked before it is sent, and the initials
    // shown when there is no picture.
    // No MAUI dependency, so MauiApp1.Tests compiles this file.
    public static class AvatarRules
    {
        public const long MaximumBytes = 5 * 1024 * 1024;

        // The file picker's types
        public static readonly string[] Extensions = { ".png", ".jpg", ".jpeg", ".webp", ".gif" };

        public enum Problem { UnsupportedType, TooLarge }

        // What is wrong with a picture, null when it can be sent
        public static Problem? Check(string? fileName, long length)
        {
            if (ContentType(fileName) == null)
            {
                return Problem.UnsupportedType;
            }
            if (length > MaximumBytes)
            {
                return Problem.TooLarge;
            }
            return null;
        }

        // The picture's MIME type, null for a type the backend refuses
        public static string? ContentType(string? fileName)
        {
            switch (Path.GetExtension(fileName ?? "").ToLowerInvariant())
            {
                case ".png": return "image/png";
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".webp": return "image/webp";
                case ".gif": return "image/gif";
                default: return null;
            }
        }

        // The resource key of each problem's message (AppResources)
        public static string MessageKey(Problem problem)
        {
            return "Avatar" + problem;
        }

        // "AS" for Anna Smith, "A" for anna@example.com without a name, "" for nobody
        public static string Initials(string? firstName, string? lastName, string? emailAddress)
        {
            string initials = string.Concat(
                new[] { firstName, lastName }
                    .Select(name => (name ?? "").Trim())
                    .Where(name => name.Length > 0)
                    .Select(name => char.ToUpperInvariant(name[0])));
            if (initials.Length == 0 && !string.IsNullOrWhiteSpace(emailAddress))
            {
                initials = char.ToUpperInvariant(emailAddress.Trim()[0]).ToString();
            }
            return initials;
        }

        // The presets of GET /avatars/presets; empty when the response isn't them
        public static List<AvatarPreset> ParsePresets(string? json)
        {
            try
            {
                var response = JsonSerializer.Deserialize<AvatarPresetsResponse>(json ?? "");
                return (response?.presets ?? new List<AvatarPreset>())
                    .Where(preset => preset.name.Length > 0 && Uri.TryCreate(preset.url, UriKind.Absolute, out _))
                    .ToList();
            }
            catch (JsonException)
            {
                return new List<AvatarPreset>();
            }
        }
    }
}

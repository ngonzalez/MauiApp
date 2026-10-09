// The MIME type of a file from its extension, application/octet-stream for a
// type the app doesn't upload. No MAUI dependency: MauiApp1.Tests compiles it.
public static class MimeTypeMapper
{
    // Case-insensitive: a camera's IMG_0001.JPG is a JPEG too
    private static readonly IDictionary<string, string> _mappings =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            /* DOCUMENTS */
            { ".pdf", "application/pdf" },
            { ".md", "text/markdown" },
            { ".txt", "text/plain" },

            /* IMAGES */
            { ".bmp", "image/bmp" },
            { ".gif", "image/gif" },
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".png", "image/png" },
            { ".tif", "image/tiff" },
            { ".tiff", "image/tiff" },
            { ".webp", "image/webp" },

            /* AUDIO */
            { ".aac", "audio/aac" },
            { ".m4a", "audio/aac" },
            { ".aff", "audio/x-aiff" },
            { ".aif", "audio/x-aiff" },
            { ".aiff", "audio/x-aiff" },
            { ".flac", "audio/flac" },
            { ".mka", "audio/x-matroska" },
            { ".mp3", "audio/mpeg" },
            { ".wav", "audio/wav" },
            { ".weba", "audio/webm" },

            /* VIDEO */
            { ".3gp", "video/3gpp" },
            { ".mkv", "video/x-matroska" },
            { ".mp4", "video/mp4" },
            { ".mp4v", "video/mp4" },
            { ".mpg4", "video/mp4" },
            { ".m1v", "video/mpeg" },
            { ".m2v", "video/mpeg" },
            { ".mpg", "video/mpeg" },
            { ".mpeg", "video/mpeg" },
            { ".webm", "video/webm" },
        };
    public static string GetMimeType(string extension)
    {
        if (extension == null)
        {
            throw new ArgumentNullException("extension");
        }
        if (!extension.StartsWith("."))
        {
            extension = "." + extension;
        }
        return _mappings.TryGetValue(extension, out string? mime) ? mime : "application/octet-stream";
    }
}

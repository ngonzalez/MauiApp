using System.Globalization;

namespace MauiApp1
{
    // Writes the exceptions nobody caught, with their stack trace, to
    // %LOCALAPPDATA%\Appshare\crash.log, so a crash can be read without the
    // debugger. No MAUI dependency, so MauiApp1.Tests compiles this file.
    public static class CrashLog
    {
        public static string DefaultPath { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Appshare", "crash.log");

        // Append one entry; never throws (it runs while the app is crashing)
        public static void Write(Exception? exception, string source, string? path = null)
        {
            try
            {
                path ??= DefaultPath;
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                string entry = string.Format(CultureInfo.InvariantCulture, "[{0:yyyy-MM-dd HH:mm:ss}] {1}{2}{3}{2}{2}",
                    DateTime.Now, source, Environment.NewLine, exception?.ToString() ?? "(no exception)");
                File.AppendAllText(path, entry);
                System.Diagnostics.Debug.WriteLine(entry);
            }
            catch
            {
                // Nothing more can be done
            }
        }
    }
}

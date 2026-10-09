using Xunit;

namespace MauiApp1.Tests
{
    public class CrashLogTests : IDisposable
    {
        private readonly string _path = Path.Combine(Path.GetTempPath(), "crash-log-" + Guid.NewGuid(), "crash.log");

        public void Dispose()
        {
            Directory.Delete(Path.GetDirectoryName(_path)!, recursive: true);
        }

        private static Exception Thrown()
        {
            try
            {
                throw new InvalidOperationException("the sign-in failed");
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        [Fact]
        public void WritesTheSourceTheExceptionAndItsStackTrace()
        {
            CrashLog.Write(Thrown(), "WinUI", _path);

            string log = File.ReadAllText(_path);
            Assert.Contains("WinUI", log);
            Assert.Contains("System.InvalidOperationException: the sign-in failed", log);
            Assert.Contains(nameof(Thrown), log);
        }

        [Fact]
        public void AppendsTheEntries()
        {
            CrashLog.Write(Thrown(), "first", _path);
            CrashLog.Write(Thrown(), "second", _path);

            string log = File.ReadAllText(_path);
            Assert.True(log.IndexOf("first") < log.IndexOf("second"));
        }

        [Fact]
        public void WritesAnEntryWithoutAnException()
        {
            CrashLog.Write(null, "Task", _path);

            Assert.Contains("(no exception)", File.ReadAllText(_path));
        }

        [Fact]
        public void NeverThrows()
        {
            // a directory where the file should be
            Directory.CreateDirectory(_path);

            CrashLog.Write(Thrown(), "WinUI", _path);
        }
    }
}

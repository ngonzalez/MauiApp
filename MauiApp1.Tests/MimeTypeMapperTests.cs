using Xunit;

namespace MauiApp1.Tests
{
    public class MimeTypeMapperTests
    {
        [Theory]
        [InlineData(".jpg", "image/jpeg")]
        [InlineData(".JPG", "image/jpeg")]
        [InlineData("JPG", "image/jpeg")]
        [InlineData(".Png", "image/png")]
        [InlineData(".MP4", "video/mp4")]
        [InlineData(".Wav", "audio/wav")]
        [InlineData(".PDF", "application/pdf")]
        public void KnowsTheTypeWhateverTheCase(string extension, string mimeType)
        {
            Assert.Equal(mimeType, MimeTypeMapper.GetMimeType(extension));
        }

        [Theory]
        [InlineData(".exe")]
        [InlineData(".EXE")]
        [InlineData("")]
        public void AnUnknownTypeIsNotUploaded(string extension)
        {
            Assert.Equal("application/octet-stream", MimeTypeMapper.GetMimeType(extension));
        }

        [Fact]
        public void RefusesNoExtension()
        {
            Assert.Throws<ArgumentNullException>(() => MimeTypeMapper.GetMimeType(null!));
        }
    }
}

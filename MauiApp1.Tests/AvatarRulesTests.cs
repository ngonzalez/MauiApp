using Xunit;
using static MauiApp1.AvatarRules;

namespace MauiApp1.Tests
{
    // The backend's rules for PUT /user/avatar: PNG, JPEG, WebP or GIF, 5 MB at most
    public class AvatarRulesTests
    {
        [Theory]
        [InlineData("me.png", "image/png")]
        [InlineData("IMG_0001.JPG", "image/jpeg")]
        [InlineData("me.jpeg", "image/jpeg")]
        [InlineData("me.webp", "image/webp")]
        [InlineData("me.gif", "image/gif")]
        public void KnowsTheTypesTheBackendAccepts(string fileName, string contentType)
        {
            Assert.Equal(contentType, ContentType(fileName));
            Assert.Null(Check(fileName, 1024));
        }

        [Theory]
        [InlineData("me.bmp")]
        [InlineData("me.heic")]
        [InlineData("me")]
        [InlineData(null)]
        public void RefusesOtherTypes(string? fileName)
        {
            Assert.Null(ContentType(fileName));
            Assert.Equal(Problem.UnsupportedType, Check(fileName, 1024));
        }

        [Fact]
        public void AcceptsFiveMegabytesButNotMore()
        {
            Assert.Null(Check("me.png", MaximumBytes));
            Assert.Equal(Problem.TooLarge, Check("me.png", MaximumBytes + 1));
        }

        [Fact]
        public void TheFilePickerOffersTheAcceptedTypes()
        {
            Assert.All(Extensions, extension => Assert.NotNull(ContentType("me" + extension)));
        }

        [Fact]
        public void MessageKeys()
        {
            Assert.Equal("AvatarTooLarge", MessageKey(Problem.TooLarge));
            Assert.Equal("AvatarUnsupportedType", MessageKey(Problem.UnsupportedType));
        }

        [Theory]
        [InlineData("Anna", "Smith", "anna@example.com", "AS")]
        [InlineData("anna", null, "anna@example.com", "A")]
        [InlineData(" ", "", "bob@example.com", "B")]
        [InlineData(null, null, null, "")]
        public void Initials(string? firstName, string? lastName, string? emailAddress, string initials)
        {
            Assert.Equal(initials, AvatarRules.Initials(firstName, lastName, emailAddress));
        }

        [Fact]
        public void ParsesThePresets()
        {
            var presets = ParsePresets("{\"presets\":[{\"name\":\"sunrise\",\"url\":\"https://api.appshare.site/avatars/presets/sunrise.png\"},{\"name\":\"\",\"url\":\"x\"}]}");
            Assert.Single(presets);
            Assert.Equal("sunrise", presets[0].name);
            Assert.Equal("https://api.appshare.site/avatars/presets/sunrise.png", presets[0].url);
        }

        [Theory]
        [InlineData("<html>Bad gateway</html>")]
        [InlineData("{}")]
        [InlineData("")]
        [InlineData(null)]
        public void NoPresetsFromAnotherResponse(string? json)
        {
            Assert.Empty(ParsePresets(json));
        }
    }
}

using System.Globalization;
using Xunit;

namespace MauiApp1.Tests
{
    // The account's plan, read from the backend's GET /account
    public class PlanUsageTests
    {
        private const string Summary = """
            {"account":{"name":"Acme","subdomain":"acme","siteUrl":"https://acme.appshare.site",
                        "plan":{"name":"5GB","storage":{"used":51357725,"limit":5368709120},"users":{"count":1,"limit":3}}},
             "user":{"name":"Anna","emailAddress":"anna@example.com","admin":false,"statusAdmin":false},
             "folders":{"created":0,"published":0,"archived":0,"total":0},
             "files":{"count":50,"byteSize":51357725,"lastUploadAt":null}}
            """;

        [Fact]
        public void ReadsThePlanOfTheAccount()
        {
            Assert.Equal(new PlanUsage("5GB", 51357725, 5368709120, 1, 3), PlanUsage.Parse(Summary));
        }

        [Theory]
        [InlineData("""{"account":{"name":"Acme","subdomain":"acme","siteUrl":"x"}}""")]
        [InlineData("""{"account":{"plan":null}}""")]
        [InlineData("""{"account":{"plan":{"name":"5GB"}}}""")]
        [InlineData("""{"user":{}}""")]
        [InlineData("<html>")]
        [InlineData("")]
        public void NoPlanFromABackendBeforePlansOrAnAnswerItCantRead(string json)
        {
            Assert.Null(PlanUsage.Parse(json));
        }

        [Fact]
        public void StorageFraction()
        {
            Assert.Equal(0.5, new PlanUsage("5GB", 2684354560, 5368709120, 1, 3).StorageFraction);
            Assert.Equal(1, new PlanUsage("5GB", 6000000000, 5368709120, 1, 3).StorageFraction);
            Assert.Equal(0, new PlanUsage("x", 0, 0, 0, 1).StorageFraction);
        }

        [Fact]
        public void IsFullWhenTheFilesOrTheUsersReachTheLimit()
        {
            Assert.False(new PlanUsage("5GB", 51357725, 5368709120, 1, 3).IsFull);
            Assert.True(new PlanUsage("5GB", 5368709120, 5368709120, 1, 3).IsFull);
            Assert.True(new PlanUsage("5GB", 0, 5368709120, 3, 3).IsFull);
        }

        [Theory]
        [InlineData(5368709120, "5 GB")]
        [InlineData(10737418240, "10 GB")]
        [InlineData(51357725, "49 MB")]
        [InlineData(5261334938, "4.9 GB")]
        [InlineData(512, "512 bytes")]
        [InlineData(0, "0 bytes")]
        public void SizesInBinaryUnitsLikeThePlansNames(long bytes, string shown)
        {
            Assert.Equal(shown, PlanUsage.FormatSize(bytes, CultureInfo.InvariantCulture));
        }

        [Fact]
        public void SizesInTheCulturesNumberFormat()
        {
            Assert.Equal("4,9 GB", PlanUsage.FormatSize(5261334938, new CultureInfo("fr-FR")));
        }
    }
}

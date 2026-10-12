using Xunit;
using static MauiApp1.PasswordRules;

namespace MauiApp1.Tests
{
    // The same cases as the backend's spec (User password rules)
    public class PasswordRulesTests
    {
        [Fact]
        public void AcceptsTwelveCharactersWithALetterAndADigit()
        {
            Assert.Empty(Check("correct-horse-7", "anna.smith@example.com"));
        }

        [Fact]
        public void RefusesAShortPassword()
        {
            Assert.Equal(new[] { Problem.TooShort }, Check("short-pass1", "anna.smith@example.com"));
        }

        [Theory]
        [InlineData("no-digits-in-here")]
        [InlineData("1234-5678-9012")]
        public void RefusesAPasswordWithoutALetterOrADigit(string password)
        {
            Assert.Equal(new[] { Problem.LetterAndDigit }, Check(password, "a@b.c"));
        }

        [Fact]
        public void RefusesTheEmailAddressNameWhateverTheCase()
        {
            Assert.Equal(new[] { Problem.ContainsEmail }, Check("My-Anna.Smith-2026", "anna.smith@example.com"));
        }

        [Fact]
        public void IgnoresAShortEmailAddressName()
        {
            Assert.Empty(Check("an-ordinary-pass1", "an@example.com"));
        }

        [Fact]
        public void AcceptsLettersWithAccents()
        {
            Assert.Empty(Check("élève-motdepasse-3", "anna@example.com"));
        }

        [Fact]
        public void AMissingPasswordOrEmailHasEveryProblemButTheEmail()
        {
            Assert.Equal(new[] { Problem.TooShort, Problem.LetterAndDigit }, Check(null, null));
        }

        [Fact]
        public void EachProblemHasAMessage()
        {
            Assert.Equal("PasswordTooShort", MessageKey(Problem.TooShort));
            Assert.Equal("PasswordContainsEmail", MessageKey(Problem.ContainsEmail));
        }
    }
}

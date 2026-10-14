namespace MauiApp1
{
    // The backend's password rules (User#password_strength), checked before a
    // password is sent: at least 12 characters, a letter and a digit, and not
    // the name of the email address (before the @, 3 letters or more).
    // No MAUI dependency, so MauiApp1.Tests compiles this file.
    public static class PasswordRules
    {
        public const int MinimumLength = 12;

        public enum Problem { TooShort, LetterAndDigit, ContainsEmail }

        // What is wrong with a password, empty when it follows the rules
        public static List<Problem> Check(string? password, string? emailAddress)
        {
            password ??= "";
            var problems = new List<Problem>();
            if (password.Length < MinimumLength)
            {
                problems.Add(Problem.TooShort);
            }
            if (!password.Any(char.IsLetter) || !password.Any(char.IsDigit))
            {
                problems.Add(Problem.LetterAndDigit);
            }
            string name = (emailAddress ?? "").Split('@')[0].ToLowerInvariant();
            if (name.Length >= 3 && password.ToLowerInvariant().Contains(name))
            {
                problems.Add(Problem.ContainsEmail);
            }
            return problems;
        }

        // The resource key of each problem's message (AppResources)
        public static string MessageKey(Problem problem)
        {
            return "Password" + problem;
        }
    }
}

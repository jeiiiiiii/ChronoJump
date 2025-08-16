using System.Text.RegularExpressions;

public static class PatternManager
{
    private static readonly Regex emailRegex = new Regex(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public static bool IsValidEmail(string email)
    {
        return !string.IsNullOrEmpty(email) && emailRegex.IsMatch(email);
    }

    private static readonly Regex PasswordRegex = new Regex(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,32}$",
        RegexOptions.Compiled
    );

    public static bool IsValidPassword(string password)
    {
        return PasswordRegex.IsMatch(password);
    }
}




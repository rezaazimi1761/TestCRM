using System.Net.Mail;
using System.Text.RegularExpressions;

namespace ModernCRM.Web.Validation;

public static class FormValidation
{
    public static string Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "Email is required";
        try { _ = new MailAddress(value); return null!; } catch { return "Enter a valid email address"; }
    }

    public static string Url(string value)
        => string.IsNullOrWhiteSpace(value) || (Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https")
            ? null! : "Enter a valid HTTP or HTTPS URL";

    public static string Phone(string value)
        => string.IsNullOrWhiteSpace(value) || Regex.IsMatch(value, "^\\+?[0-9][0-9 ()-]{6,29}$") ? null! : "Enter a valid phone number";

    public static string Slug(string value)
        => !string.IsNullOrWhiteSpace(value) && Regex.IsMatch(value, "^[a-z0-9]+(?:-[a-z0-9]+)*$") ? null! : "Use lowercase letters, numbers and single hyphens";

    public static string Username(string value)
        => !string.IsNullOrWhiteSpace(value) && value.Length is >= 3 and <= 100 && Regex.IsMatch(value, "^[a-zA-Z0-9][a-zA-Z0-9._-]*$") ? null! : "Username must be 3-100 characters and may contain . _ -";

    public static string Password(string value)
        => !string.IsNullOrWhiteSpace(value) && value.Length is >= 8 and <= 128 && Regex.IsMatch(value, "[A-Z]") && Regex.IsMatch(value, "[a-z]") && Regex.IsMatch(value, "[0-9]") && Regex.IsMatch(value, "[^a-zA-Z0-9]")
            ? null! : "Use 8+ characters with uppercase, lowercase, number and symbol";

    public static string NonNegative(decimal value) => value >= 0 ? null! : "Value cannot be negative";
    public static string NotPast(DateTime? value) => !value.HasValue || value.Value.Date >= DateTime.Today ? null! : "Date cannot be in the past";
}

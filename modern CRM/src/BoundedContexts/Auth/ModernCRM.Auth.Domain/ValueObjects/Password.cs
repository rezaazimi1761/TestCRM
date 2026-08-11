using System.Text.RegularExpressions;
using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Auth.Domain.ValueObjects;

public sealed record Password : ValueObject
{
    public string Value { get; }
    private Password(string value) => Value = value;

    public static Password Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length is < 8 or > 128)
            throw new BusinessRuleValidationException("Password must be between 8 and 128 characters.");
        if (!Regex.IsMatch(value, "[A-Z]") || !Regex.IsMatch(value, "[a-z]") || !Regex.IsMatch(value, "[0-9]") || !Regex.IsMatch(value, "[^a-zA-Z0-9]"))
            throw new BusinessRuleValidationException("Password must include uppercase, lowercase, number and special character.");
        return new Password(value);
    }
}

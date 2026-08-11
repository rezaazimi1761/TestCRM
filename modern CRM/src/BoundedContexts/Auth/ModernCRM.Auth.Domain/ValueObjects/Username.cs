using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Auth.Domain.ValueObjects;

public sealed record Username : ValueObject
{
    public string Value { get; }
    private Username(string value) => Value = value;

    public static Username Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Username is required.", nameof(value));
        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length is < 3 or > 100) throw new BusinessRuleValidationException("Username must be between 3 and 100 characters.");
        if (!System.Text.RegularExpressions.Regex.IsMatch(normalized, "^[a-z0-9][a-z0-9._-]*$")) throw new BusinessRuleValidationException("Username contains invalid characters.");
        return new Username(normalized);
    }

    public override string ToString() => Value;
}

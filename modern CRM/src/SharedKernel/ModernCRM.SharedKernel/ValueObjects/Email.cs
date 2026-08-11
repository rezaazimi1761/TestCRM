using System.Text.RegularExpressions;
using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.SharedKernel.ValueObjects;

public sealed record Email : ValueObject
{
    private static readonly Regex Pattern = new("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$", RegexOptions.Compiled);
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > 200 || !Pattern.IsMatch(value.Trim()))
            throw new ArgumentException("Email is not valid.", nameof(value));
        return new Email(value.Trim().ToLowerInvariant());
    }

    public override string ToString() => Value;
}

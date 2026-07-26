using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Auth.Domain.ValueObjects;

public sealed record Username : ValueObject
{
    public string Value { get; }
    private Username(string value) => Value = value;

    public static Username Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Username is required.", nameof(value));
        if (value.Length > 100) throw new ArgumentException("Username is too long.", nameof(value));
        return new Username(value.Trim().ToLowerInvariant());
    }

    public override string ToString() => Value;
}

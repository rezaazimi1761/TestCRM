using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Auth.Domain.ValueObjects;

public sealed record PasswordHash : ValueObject
{
    public string Value { get; }
    private PasswordHash(string value) => Value = value;
    public static PasswordHash FromHash(string hash) => string.IsNullOrWhiteSpace(hash)
        ? throw new ArgumentException("Password hash is required.", nameof(hash))
        : new PasswordHash(hash);
    public override string ToString() => Value;
}

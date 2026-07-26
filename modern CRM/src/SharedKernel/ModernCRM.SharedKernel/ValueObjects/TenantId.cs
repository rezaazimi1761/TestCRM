using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.SharedKernel.ValueObjects;

public sealed record TenantId : ValueObject
{
    public string Value { get; }

    private TenantId(string value) => Value = value;

    public static TenantId Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Tenant id is required.", nameof(value));
        return new TenantId(value.Trim().ToLowerInvariant());
    }

    public override string ToString() => Value;
}

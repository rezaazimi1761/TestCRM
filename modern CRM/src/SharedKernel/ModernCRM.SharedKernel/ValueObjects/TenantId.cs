using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.SharedKernel.ValueObjects;

public sealed record TenantId : ValueObject
{
    public string Value { get; }

    private TenantId(string value) => Value = value;

    public static TenantId Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Tenant id is required.", nameof(value));
        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length > 100 || !System.Text.RegularExpressions.Regex.IsMatch(normalized, "^[a-z0-9]+(?:-[a-z0-9]+)*$"))
            throw new BusinessRuleValidationException("Tenant id must contain only lowercase letters, numbers and single hyphens.");
        return new TenantId(normalized);
    }

    public override string ToString() => Value;
}

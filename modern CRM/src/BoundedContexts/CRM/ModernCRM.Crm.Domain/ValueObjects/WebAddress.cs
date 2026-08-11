using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Crm.Domain.ValueObjects;

public sealed record WebAddress : ValueObject
{
    public string Value { get; }
    private WebAddress(string value) => Value = value;
    public static WebAddress Create(string value)
    {
        if (!Uri.TryCreate(value?.Trim(), UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https"))
            throw new BusinessRuleValidationException("Website must be a valid HTTP or HTTPS URL.");
        if (uri.AbsoluteUri.Length > 500) throw new BusinessRuleValidationException("Website cannot exceed 500 characters.");
        return new WebAddress(uri.AbsoluteUri);
    }
}

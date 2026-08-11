using System.Text.RegularExpressions;
using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Crm.Domain.ValueObjects;

public sealed record PhoneNumber : ValueObject
{
    public string Value { get; }
    private PhoneNumber(string value) => Value = value;
    public static PhoneNumber Create(string value)
    {
        var normalized = value.Trim();
        if (normalized.Length > 30 || !Regex.IsMatch(normalized, "^\\+?[0-9][0-9 ()-]{6,29}$"))
            throw new BusinessRuleValidationException("Phone number is invalid.");
        return new PhoneNumber(normalized);
    }
}

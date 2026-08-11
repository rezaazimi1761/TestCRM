using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Crm.Domain.ValueObjects;

public sealed record Money(decimal Amount, string Currency) : ValueObject
{
    public static Money Create(decimal amount, string currency = "USD")
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        var normalizedCurrency = currency?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalizedCurrency) || normalizedCurrency.Length != 3 || !normalizedCurrency.All(char.IsLetter))
            throw new BusinessRuleValidationException("Currency must be a three-letter ISO code.");
        return new Money(amount, normalizedCurrency);
    }
}

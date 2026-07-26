using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Crm.Domain.ValueObjects;

public sealed record Money(decimal Amount, string Currency) : ValueObject
{
    public static Money Create(decimal amount, string currency = "USD")
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency is required.", nameof(currency));
        return new Money(amount, currency.Trim().ToUpperInvariant());
    }
}

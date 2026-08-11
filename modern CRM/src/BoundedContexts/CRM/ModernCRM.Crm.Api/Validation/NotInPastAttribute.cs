using System.ComponentModel.DataAnnotations;

namespace ModernCRM.Crm.Api.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class NotInPastAttribute : ValidationAttribute
{
    public NotInPastAttribute() => ErrorMessage = "Date cannot be in the past.";
    public override bool IsValid(object? value) => value is null || value is DateTime date && date.Date >= DateTime.UtcNow.Date;
}

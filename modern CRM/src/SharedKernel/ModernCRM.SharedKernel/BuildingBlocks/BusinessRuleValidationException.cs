namespace ModernCRM.SharedKernel.BuildingBlocks;

public sealed class BusinessRuleValidationException : Exception
{
    public BusinessRuleValidationException(string message) : base(message) { }
}

namespace ModernCRM.SharedKernel.BuildingBlocks;

public static class Guard
{
    public static string NotBlank(string? value, string name, int maxLength = 500)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new BusinessRuleValidationException($"{name} is required.");
        var result = value.Trim();
        if (result.Length > maxLength) throw new BusinessRuleValidationException($"{name} cannot exceed {maxLength} characters.");
        return result;
    }

    public static void Against(bool condition, string message)
    {
        if (condition) throw new BusinessRuleValidationException(message);
    }
}

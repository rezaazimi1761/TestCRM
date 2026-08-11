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

    public static string? OptionalText(string? value, string name, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var result = value.Trim();
        if (result.Length > maxLength) throw new BusinessRuleValidationException($"{name} cannot exceed {maxLength} characters.");
        return result;
    }

    public static T ValidEnum<T>(string value, string name) where T : struct, Enum
        => Enum.TryParse<T>(value, true, out var result) && Enum.IsDefined(result)
            ? result
            : throw new BusinessRuleValidationException($"{name} is invalid.");
}

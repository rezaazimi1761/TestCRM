using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Auth.Domain.Users;

public sealed class UserClaim : Entity<int>
{
    public string Type { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    private UserClaim() { }
    public UserClaim(string type, string value)
    {
        Type = Guard.NotBlank(type, nameof(Type), 200);
        Value = Guard.NotBlank(value, nameof(Value), 500);
    }
}

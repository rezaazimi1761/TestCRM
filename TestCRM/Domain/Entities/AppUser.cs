using Shared.Domain.Common;

namespace TestCRM.Domain.Entities;

public class AppUser : BaseEntity
{
    public string Username     { get; set; } = string.Empty;
    public string FirstName    { get; set; } = string.Empty;
    public string LastName     { get; set; } = string.Empty;
    public string Email        { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role         { get; set; } = "User";
    public bool   IsActive     { get; set; } = true;
}

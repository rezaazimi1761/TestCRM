namespace AuthService.Domain.Entities;

public class AppUser
{
    public int    Id           { get; set; }
    public string TenantId     { get; set; } = default!;
    public string Username     { get; set; } = default!;
    public string Email        { get; set; } = default!;
    public string FirstName    { get; set; } = default!;
    public string LastName     { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string Role         { get; set; } = "User";
    public bool   IsActive     { get; set; } = true;
    public bool   IsDeleted    { get; set; }
    public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<UserClaim>    Claims        { get; set; } = new List<UserClaim>();
}

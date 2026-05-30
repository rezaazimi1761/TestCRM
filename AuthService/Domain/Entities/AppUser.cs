namespace AuthService.Domain.Entities;

public class AppUser
{
    public int    Id           { get; set; }

    /// <summary>FK to Tenant.Slug — also acts as the multi-tenancy discriminator.</summary>
    public string TenantId     { get; set; } = default!;

    public string Username     { get; set; } = default!;
    public string Email        { get; set; } = default!;
    public string FirstName    { get; set; } = default!;
    public string LastName     { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;

    /// <summary>Roles: User | Admin | SuperUser</summary>
    public string Role         { get; set; } = "User";

    public bool     IsActive   { get; set; } = true;
    public bool     IsDeleted  { get; set; }
    public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;

    // ── Navigation ────────────────────────────────────────────────
    public Tenant?   Tenant        { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<UserClaim>    Claims        { get; set; } = new List<UserClaim>();
}

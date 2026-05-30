namespace AuthService.Domain.Entities;

public class UserClaim
{
    public int    Id         { get; set; }
    public string ClaimType  { get; set; } = default!;
    public string ClaimValue { get; set; } = default!;

    public int     UserId { get; set; }
    public AppUser User   { get; set; } = default!;
}

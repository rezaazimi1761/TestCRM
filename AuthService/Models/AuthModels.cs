namespace AuthService.Models;

// ── Requests ───────────────────────────────────────────────────
public record RegisterRequest(
    string TenantId,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string Role = "User");

public record LoginRequest(
    string TenantId,
    string Username,
    string Password);

public record RefreshRequest(
    string RefreshToken);

public record AddClaimRequest(
    string Type,
    string Value);

public record ReplaceClaimsRequest(
    List<ClaimItem> Claims);

public record ClaimItem(string Type, string Value);

// ── Responses ──────────────────────────────────────────────────
public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    int    UserId,
    string Username,
    string Role,
    string TenantId);

using Shared.Domain.Common;

namespace TestCRM.Domain.Entities;

public class AppUser : BaseEntity
{
    public string Username       { get; set; } = string.Empty;
    public string FirstName      { get; set; } = string.Empty;
    public string LastName       { get; set; } = string.Empty;
    public string Email          { get; set; } = string.Empty;
    public string PasswordHash   { get; set; } = string.Empty;
    public string Role           { get; set; } = "User";
    public bool   IsActive       { get; set; } = true;

    /// <summary>
    /// Tracks whether AuthService successfully created the matching auth account.
    /// Pending → event published, waiting for AuthService confirmation
    /// Synced  → AuthService confirmed (UserAuthSyncedEvent received)
    /// Failed  → AuthService failed all retries (Fault received) → user soft-deleted
    /// </summary>
    public string AuthSyncStatus { get; set; } = AuthSync.Pending;
}

/// <summary>Constants for AuthSyncStatus — avoids magic strings across the codebase.</summary>
public static class AuthSync
{
    public const string Pending = "Pending";
    public const string Synced  = "Synced";
    public const string Failed  = "Failed";
}

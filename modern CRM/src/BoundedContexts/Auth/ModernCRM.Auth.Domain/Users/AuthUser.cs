using ModernCRM.Auth.Domain.Roles;
using ModernCRM.Auth.Domain.ValueObjects;
using ModernCRM.SharedKernel.BuildingBlocks;
using ModernCRM.SharedKernel.ValueObjects;

namespace ModernCRM.Auth.Domain.Users;

public sealed class AuthUser : AggregateRoot<int>
{
    private readonly List<UserClaim> _claims = new();

    public TenantId TenantId { get; private set; } = null!;
    public Username Username { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public PasswordHash PasswordHash { get; private set; } = null!;
    public Role Role { get; private set; } = Role.User;
    public bool IsActive { get; private set; } = true;
    public bool IsDeleted { get; private set; }
    public string IntegrationStatus { get; private set; } = "Pending";
    public string? IntegrationError { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public IReadOnlyCollection<UserClaim> Claims => _claims.AsReadOnly();

    private AuthUser() { }

    public static AuthUser Register(TenantId tenantId, Username username, Email email, string firstName, string lastName, PasswordHash passwordHash, Role role)
    {
        Guard.Against(role == Role.SuperUser && tenantId.Value != "default", "SuperUser must be created in the home/default tenant.");
        var user = new AuthUser { TenantId = tenantId, Username = username, Email = email, PasswordHash = passwordHash, Role = role, IsActive = true, CreatedAtUtc = DateTime.UtcNow };
        user.ChangeName(firstName, lastName);
        user.Raise(new AuthUserCreatedDomainEvent(user.Id, tenantId.Value, username.Value));
        return user;
    }

    public static AuthUser Rehydrate(int id, TenantId tenantId, Username username, Email email, string firstName, string lastName, PasswordHash passwordHash, Role role, bool isActive, DateTime createdAtUtc)
    {
        Guard.Against(id <= 0, "User id is invalid.");
        var user = new AuthUser { Id = id, TenantId = tenantId, Username = username, Email = email, PasswordHash = passwordHash, Role = role, IsActive = isActive, CreatedAtUtc = createdAtUtc };
        user.ChangeName(firstName, lastName);
        user.ClearDomainEvents();
        return user;
    }

    public void ChangeName(string firstName, string lastName)
    {
        EnsureNotDeleted();
        FirstName = Guard.NotBlank(firstName, nameof(FirstName), 100);
        LastName = Guard.NotBlank(lastName, nameof(LastName), 100);
        Touch();
    }

    public void ChangeEmail(Email email)
    {
        EnsureNotDeleted();
        Email = email;
        MarkIntegrationPending();
        Touch();
    }

    public void ChangeRole(Role role, bool actorIsSuperUser)
    {
        EnsureNotDeleted();
        Guard.Against(role is Role.Admin or Role.SuperUser && !actorIsSuperUser, "Only SuperUser can assign elevated roles.");
        Role = role;
        MarkIntegrationPending();
        Touch();
    }

    public void Activate() { EnsureNotDeleted(); IsActive = true; MarkIntegrationPending(); Touch(); }
    public void Deactivate() { EnsureNotDeleted(); IsActive = false; MarkIntegrationPending(); Touch(); }

    public void AddClaim(string type, string value)
    {
        EnsureNotDeleted();
        var claim = new UserClaim(type, value);
        Guard.Against(_claims.Any(c => c.Type == claim.Type && c.Value == claim.Value), "Duplicate user claim.");
        _claims.Add(claim);
        Touch();
    }

    public void RemoveClaim(int claimId)
    {
        EnsureNotDeleted();
        var claim = _claims.FirstOrDefault(c => c.Id == claimId);
        if (claim is not null) _claims.Remove(claim);
        Touch();
    }

    public void LogicalDelete()
    {
        if (IsDeleted) return;
        IsActive = false;
        IsDeleted = true;
        MarkIntegrationPending();
        Touch();
        Raise(new AuthUserDeletedDomainEvent(Id, TenantId.Value, Username.Value));
    }

    public void MarkIntegrationPending() { IntegrationStatus = "Pending"; IntegrationError = null; }
    public void MarkIntegrationSynced() { IntegrationStatus = "Synced"; IntegrationError = null; Touch(); }
    public void MarkIntegrationFailed(string error) { IsActive = false; IsDeleted = true; IntegrationStatus = "Failed"; IntegrationError = Guard.NotBlank(error, nameof(error), 2000); Touch(); }

    private void EnsureNotDeleted() => Guard.Against(IsDeleted, "Deleted user cannot be changed.");
}

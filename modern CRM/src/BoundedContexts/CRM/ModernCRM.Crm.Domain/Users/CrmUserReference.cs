using ModernCRM.SharedKernel.BuildingBlocks;
using ModernCRM.SharedKernel.ValueObjects;

namespace ModernCRM.Crm.Domain.Users;

public sealed class CrmUserReference : AggregateRoot<int>
{
    public int AuthUserId { get; private set; }
    public TenantId TenantId { get; private set; } = null!;
    public string Username { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public Email Email { get; private set; } = null!;
    public string Role { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public bool IsDeleted { get; private set; }

    private CrmUserReference() { }

    public static CrmUserReference Create(int authUserId, TenantId tenantId, string username, string firstName, string lastName, Email email, string role, bool isActive)
    {
        Guard.Against(authUserId <= 0, "Auth user id is invalid.");
        var reference = new CrmUserReference { AuthUserId = authUserId, TenantId = tenantId, Email = email };
        reference.UpdateFromAuth(username, firstName, lastName, email, role, isActive);
        return reference;
    }

    public void UpdateFromAuth(string username, string firstName, string lastName, Email email, string role, bool isActive)
    {
        Username = Guard.NotBlank(username, nameof(Username), 100);
        FullName = $"{Guard.NotBlank(firstName, nameof(firstName), 100)} {Guard.NotBlank(lastName, nameof(lastName), 100)}";
        Email = email;
        Role = Guard.NotBlank(role, nameof(Role), 50);
        IsActive = isActive;
        IsDeleted = false;
        Touch();
    }

    public void MarkDeleted() { IsActive = false; IsDeleted = true; Touch(); }
}

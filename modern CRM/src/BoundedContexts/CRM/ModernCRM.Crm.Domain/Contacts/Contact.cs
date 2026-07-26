using ModernCRM.SharedKernel.BuildingBlocks;
using ModernCRM.SharedKernel.ValueObjects;

namespace ModernCRM.Crm.Domain.Contacts;

public sealed class Contact : AggregateRoot<int>
{
    public TenantId TenantId { get; private set; } = null!;
    public int? AccountId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public Email Email { get; private set; } = null!;
    public string? Phone { get; private set; }
    public string? JobTitle { get; private set; }
    public bool IsDeleted { get; private set; }

    private Contact() { }

    public static Contact Create(TenantId tenantId, string firstName, string lastName, Email email, string? phone = null)
    {
        var contact = new Contact { TenantId = tenantId, Email = email };
        contact.ChangeName(firstName, lastName);
        contact.ChangePhone(phone);
        contact.Raise(new ContactCreatedDomainEvent(contact.Id, tenantId.Value, email.Value));
        return contact;
    }

    public void ChangeName(string firstName, string lastName)
    {
        EnsureActive();
        FirstName = Guard.NotBlank(firstName, nameof(FirstName), 100);
        LastName = Guard.NotBlank(lastName, nameof(LastName), 100);
        Touch();
    }

    public void ChangeEmail(Email email) { EnsureActive(); Email = email; Touch(); }
    public void ChangePhone(string? phone) { EnsureActive(); Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(); Touch(); }
    public void ChangeJobTitle(string? jobTitle) { EnsureActive(); JobTitle = string.IsNullOrWhiteSpace(jobTitle) ? null : jobTitle.Trim(); Touch(); }

    public void AssignToAccount(int accountId)
    {
        EnsureActive();
        Guard.Against(accountId <= 0, "Account id is invalid.");
        AccountId = accountId;
        Touch();
        Raise(new ContactAssignedToAccountDomainEvent(Id, accountId));
    }

    public void Delete()
    {
        if (IsDeleted) return;
        IsDeleted = true;
        Touch();
        Raise(new ContactDeletedDomainEvent(Id));
    }

    private void EnsureActive() => Guard.Against(IsDeleted, "Deleted contact cannot be changed.");
}

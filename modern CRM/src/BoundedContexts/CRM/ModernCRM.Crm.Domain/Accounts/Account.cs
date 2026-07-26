using ModernCRM.SharedKernel.BuildingBlocks;
using ModernCRM.SharedKernel.ValueObjects;

namespace ModernCRM.Crm.Domain.Accounts;

public sealed class Account : AggregateRoot<int>
{
    private readonly List<int> _contactIds = new();

    public TenantId TenantId { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string? Industry { get; private set; }
    public string? Website { get; private set; }
    public string? Phone { get; private set; }
    public string? Address { get; private set; }
    public bool IsDeleted { get; private set; }
    public IReadOnlyCollection<int> ContactIds => _contactIds.AsReadOnly();

    private Account() { }

    public static Account Create(TenantId tenantId, string name, string? industry = null, string? website = null)
    {
        var account = new Account { TenantId = tenantId };
        account.Rename(name);
        account.ChangeProfile(industry, website, null, null);
        account.Raise(new AccountCreatedDomainEvent(account.Id, tenantId.Value, account.Name));
        return account;
    }

    public void Rename(string name)
    {
        EnsureActive();
        Name = Guard.NotBlank(name, nameof(Name), 255);
        Touch();
        Raise(new AccountRenamedDomainEvent(Id, Name));
    }

    public void ChangeProfile(string? industry, string? website, string? phone, string? address)
    {
        EnsureActive();
        Industry = string.IsNullOrWhiteSpace(industry) ? null : industry.Trim();
        Website = string.IsNullOrWhiteSpace(website) ? null : website.Trim();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
        Touch();
    }

    public void AttachContact(int contactId)
    {
        EnsureActive();
        Guard.Against(contactId <= 0, "Contact id is invalid.");
        if (!_contactIds.Contains(contactId)) _contactIds.Add(contactId);
        Touch();
    }

    public void Delete()
    {
        if (IsDeleted) return;
        IsDeleted = true;
        Touch();
        Raise(new AccountDeletedDomainEvent(Id));
    }

    private void EnsureActive() => Guard.Against(IsDeleted, "Deleted account cannot be changed.");
}

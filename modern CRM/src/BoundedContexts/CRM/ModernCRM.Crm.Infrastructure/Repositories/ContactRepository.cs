using ModernCRM.Crm.Domain.Contacts;
using ModernCRM.Crm.Domain.Repositories;
using ModernCRM.Crm.Infrastructure.Persistence;
using ModernCRM.SharedKernel.BuildingBlocks;
using ModernCRM.SharedKernel.ValueObjects;
using System.Reflection;

namespace ModernCRM.Crm.Infrastructure.Repositories;

public sealed class ContactRepository : IContactRepository
{
    private readonly CrmDbContext _db;
    public ContactRepository(CrmDbContext db) => _db = db;

    public Task AddAsync(Contact contact, CancellationToken ct)
    {
        if (contact.Id == 0) SetId(contact, _db.NextContactId());
        _db.Contacts.Add(contact);
        return Task.CompletedTask;
    }

    public Task<Contact?> GetAsync(int id, CancellationToken ct) => Task.FromResult(_db.Contacts.FirstOrDefault(x => x.Id == id));

    public Task<IReadOnlyList<Contact>> ListAsync(TenantId tenantId, string? search, CancellationToken ct)
    {
        IEnumerable<Contact> query = _db.Contacts.Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                x.FirstName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                x.LastName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                x.Email.Value.Contains(term, StringComparison.OrdinalIgnoreCase));
        }
        return Task.FromResult<IReadOnlyList<Contact>>(query.OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList());
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);

    private static void SetId<TEntity>(TEntity entity, int id) where TEntity : Entity<int>
        => typeof(Entity<int>).GetProperty(nameof(Entity<int>.Id), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(entity, id);
}
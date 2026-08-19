using ModernCRM.Crm.Domain.Contacts;
using ModernCRM.Crm.Domain.Repositories;
using ModernCRM.Crm.Infrastructure.Persistence;
using ModernCRM.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Crm.Infrastructure.Repositories;

public sealed class ContactRepository : IContactRepository
{
    private readonly CrmDbContext _db;
    public ContactRepository(CrmDbContext db) { _db = db; UnitOfWork = new EfUnitOfWork(db); }
    public IUnitOfWork UnitOfWork { get; }

    public async Task AddAsync(Contact contact, CancellationToken ct) => await _db.Contacts.AddAsync(contact, ct);

    public Task<Contact?> GetAsync(TenantId tenantId, int id, CancellationToken ct) => _db.Contacts.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

    public async Task<IReadOnlyList<Contact>> ListAsync(TenantId tenantId, string? search, CancellationToken ct)
    {
        var query = _db.Contacts.Where(x => x.TenantId == tenantId && !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.FirstName.Contains(term) || x.LastName.Contains(term));
        }
        return await query.AsNoTracking().OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToListAsync(ct);
    }

}

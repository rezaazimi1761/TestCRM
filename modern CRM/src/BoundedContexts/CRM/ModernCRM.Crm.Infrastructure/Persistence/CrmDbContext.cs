using ModernCRM.Crm.Domain.Accounts;
using ModernCRM.Crm.Domain.Contacts;
using ModernCRM.Crm.Domain.Opportunities;
using ModernCRM.Crm.Domain.Tickets;
using ModernCRM.Crm.Domain.Users;

namespace ModernCRM.Crm.Infrastructure.Persistence;

public sealed class CrmDbContext
{
    private int _accountIdSequence;
    private int _contactIdSequence;
    private int _ticketIdSequence;
    private int _opportunityIdSequence;
    private int _userReferenceIdSequence;

    public List<Account> Accounts { get; } = new();
    public List<Contact> Contacts { get; } = new();
    public List<Ticket> Tickets { get; } = new();
    public List<Opportunity> Opportunities { get; } = new();
    public List<CrmUserReference> UserReferences { get; } = new();
    public List<object> OutboxMessages { get; } = new();
    public List<object> InboxMessages { get; } = new();

    public int NextAccountId() => Interlocked.Increment(ref _accountIdSequence);
    public int NextContactId() => Interlocked.Increment(ref _contactIdSequence);
    public int NextTicketId() => Interlocked.Increment(ref _ticketIdSequence);
    public int NextOpportunityId() => Interlocked.Increment(ref _opportunityIdSequence);
    public int NextUserReferenceId() => Interlocked.Increment(ref _userReferenceIdSequence);

    public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
}
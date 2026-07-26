using System.Reflection;

namespace ModernCRM.Crm.Api.Frontend;

public sealed class FrontendCrmStore
{
    private int _nextId;
    public object SyncRoot { get; } = new();
    public List<AccountModel> Accounts { get; } = new();
    public List<ContactModel> Contacts { get; } = new();
    public List<LeadModel> Leads { get; } = new();
    public List<OpportunityModel> Opportunities { get; } = new();
    public List<TicketModel> Tickets { get; } = new();
    public List<ActivityModel> Activities { get; } = new();
    public int NextId() => Interlocked.Increment(ref _nextId);
}

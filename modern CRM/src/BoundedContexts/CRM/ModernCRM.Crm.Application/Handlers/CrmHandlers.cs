using ModernCRM.Crm.Application.Commands;
using ModernCRM.Crm.Application.DTO;
using ModernCRM.Crm.Application.Queries;
using ModernCRM.Crm.Domain.Accounts;
using ModernCRM.Crm.Domain.Contacts;
using ModernCRM.Crm.Domain.Opportunities;
using ModernCRM.Crm.Domain.Repositories;
using ModernCRM.Crm.Domain.Tickets;
using ModernCRM.Crm.Domain.ValueObjects;
using ModernCRM.SharedKernel.Application;
using ModernCRM.SharedKernel.ValueObjects;

namespace ModernCRM.Crm.Application.Handlers;

public sealed class CreateAccountHandler : ICommandHandler<CreateAccountCommand, int>
{
    private readonly IAccountRepository _accounts;
    public CreateAccountHandler(IAccountRepository accounts) => _accounts = accounts;

    public async Task<int> Handle(CreateAccountCommand command, CancellationToken ct)
    {
        var account = Account.Create(TenantId.Create(command.TenantId), command.Name, command.Industry, command.Website);
        account.ChangeProfile(command.Industry, command.Website, command.Phone, command.Address);
        await _accounts.AddAsync(account, ct);
        await _accounts.SaveChangesAsync(ct);
        return account.Id;
    }
}

public sealed class UpdateAccountHandler : ICommandHandler<UpdateAccountCommand, bool>
{
    private readonly IAccountRepository _accounts;
    public UpdateAccountHandler(IAccountRepository accounts) => _accounts = accounts;

    public async Task<bool> Handle(UpdateAccountCommand command, CancellationToken ct)
    {
        var account = await _accounts.GetAsync(command.Id, ct);
        if (account is null) return false;
        account.Rename(command.Name);
        account.ChangeProfile(command.Industry, command.Website, command.Phone, command.Address);
        await _accounts.SaveChangesAsync(ct);
        return true;
    }
}

public sealed class DeleteAccountHandler : ICommandHandler<DeleteAccountCommand, bool>
{
    private readonly IAccountRepository _accounts;
    public DeleteAccountHandler(IAccountRepository accounts) => _accounts = accounts;

    public async Task<bool> Handle(DeleteAccountCommand command, CancellationToken ct)
    {
        var account = await _accounts.GetAsync(command.Id, ct);
        if (account is null) return false;
        account.Delete();
        await _accounts.SaveChangesAsync(ct);
        return true;
    }
}

public sealed class GetAccountsHandler : IQueryHandler<GetAccountsQuery, IReadOnlyList<AccountDto>>
{
    private readonly IAccountRepository _accounts;
    public GetAccountsHandler(IAccountRepository accounts) => _accounts = accounts;

    public async Task<IReadOnlyList<AccountDto>> Handle(GetAccountsQuery query, CancellationToken ct)
    {
        var items = await _accounts.ListAsync(TenantId.Create(query.TenantId), query.Search, ct);
        return items.Select(x => new AccountDto(x.Id, x.TenantId.Value, x.Name, x.Industry, x.Website, x.Phone, x.Address)).ToList();
    }
}

public sealed class GetAccountByIdHandler : IQueryHandler<GetAccountByIdQuery, AccountDto?>
{
    private readonly IAccountRepository _accounts;
    public GetAccountByIdHandler(IAccountRepository accounts) => _accounts = accounts;

    public async Task<AccountDto?> Handle(GetAccountByIdQuery query, CancellationToken ct)
    {
        var x = await _accounts.GetAsync(query.Id, ct);
        return x is null || x.IsDeleted ? null : new AccountDto(x.Id, x.TenantId.Value, x.Name, x.Industry, x.Website, x.Phone, x.Address);
    }
}

public sealed class CreateContactHandler : ICommandHandler<CreateContactCommand, int>
{
    private readonly IContactRepository _contacts;
    public CreateContactHandler(IContactRepository contacts) => _contacts = contacts;

    public async Task<int> Handle(CreateContactCommand command, CancellationToken ct)
    {
        var contact = Contact.Create(TenantId.Create(command.TenantId), command.FirstName, command.LastName, Email.Create(command.Email), command.Phone);
        contact.ChangeJobTitle(command.JobTitle);
        if (command.AccountId is > 0) contact.AssignToAccount(command.AccountId.Value);
        await _contacts.AddAsync(contact, ct);
        await _contacts.SaveChangesAsync(ct);
        return contact.Id;
    }
}

public sealed class UpdateContactHandler : ICommandHandler<UpdateContactCommand, bool>
{
    private readonly IContactRepository _contacts;
    public UpdateContactHandler(IContactRepository contacts) => _contacts = contacts;

    public async Task<bool> Handle(UpdateContactCommand command, CancellationToken ct)
    {
        var contact = await _contacts.GetAsync(command.Id, ct);
        if (contact is null) return false;
        contact.ChangeName(command.FirstName, command.LastName);
        contact.ChangeEmail(Email.Create(command.Email));
        contact.ChangePhone(command.Phone);
        contact.ChangeJobTitle(command.JobTitle);
        if (command.AccountId is > 0) contact.AssignToAccount(command.AccountId.Value);
        await _contacts.SaveChangesAsync(ct);
        return true;
    }
}

public sealed class DeleteContactHandler : ICommandHandler<DeleteContactCommand, bool>
{
    private readonly IContactRepository _contacts;
    public DeleteContactHandler(IContactRepository contacts) => _contacts = contacts;

    public async Task<bool> Handle(DeleteContactCommand command, CancellationToken ct)
    {
        var contact = await _contacts.GetAsync(command.Id, ct);
        if (contact is null) return false;
        contact.Delete();
        await _contacts.SaveChangesAsync(ct);
        return true;
    }
}

public sealed class GetContactsHandler : IQueryHandler<GetContactsQuery, IReadOnlyList<ContactDto>>
{
    private readonly IContactRepository _contacts;
    public GetContactsHandler(IContactRepository contacts) => _contacts = contacts;

    public async Task<IReadOnlyList<ContactDto>> Handle(GetContactsQuery query, CancellationToken ct)
    {
        var items = await _contacts.ListAsync(TenantId.Create(query.TenantId), query.Search, ct);
        return items.Select(x => new ContactDto(x.Id, x.TenantId.Value, x.FirstName, x.LastName, x.Email.Value, x.Phone, x.JobTitle, x.AccountId)).ToList();
    }
}

public sealed class GetContactByIdHandler : IQueryHandler<GetContactByIdQuery, ContactDto?>
{
    private readonly IContactRepository _contacts;
    public GetContactByIdHandler(IContactRepository contacts) => _contacts = contacts;

    public async Task<ContactDto?> Handle(GetContactByIdQuery query, CancellationToken ct)
    {
        var x = await _contacts.GetAsync(query.Id, ct);
        return x is null || x.IsDeleted ? null : new ContactDto(x.Id, x.TenantId.Value, x.FirstName, x.LastName, x.Email.Value, x.Phone, x.JobTitle, x.AccountId);
    }
}

public sealed class CreateTicketHandler : ICommandHandler<CreateTicketCommand, int>
{
    private readonly ITicketRepository _tickets;
    public CreateTicketHandler(ITicketRepository tickets) => _tickets = tickets;

    public async Task<int> Handle(CreateTicketCommand command, CancellationToken ct)
    {
        var priority = Enum.Parse<TicketPriority>(command.Priority, true);
        var ticket = Ticket.Create(TenantId.Create(command.TenantId), command.AccountId, command.Subject, priority, command.DueDate);
        ticket.ChangeDescription(command.Description);
        if (command.ContactId is > 0) ticket.LinkContact(command.ContactId.Value);
        if (command.AssignedToAuthUserId is > 0) ticket.AssignToUser(command.AssignedToAuthUserId.Value);
        await _tickets.AddAsync(ticket, ct);
        await _tickets.SaveChangesAsync(ct);
        return ticket.Id;
    }
}

public sealed class UpdateTicketHandler : ICommandHandler<UpdateTicketCommand, bool>
{
    private readonly ITicketRepository _tickets;
    public UpdateTicketHandler(ITicketRepository tickets) => _tickets = tickets;

    public async Task<bool> Handle(UpdateTicketCommand command, CancellationToken ct)
    {
        var ticket = await _tickets.GetAsync(command.Id, ct);
        if (ticket is null) return false;

        ticket.ChangeSubject(command.Subject);
        ticket.ChangeDescription(command.Description);
        ticket.ChangePriority(Enum.Parse<TicketPriority>(command.Priority, true));
        if (command.ContactId is > 0) ticket.LinkContact(command.ContactId.Value);
        if (command.AssignedToAuthUserId is > 0) ticket.AssignToUser(command.AssignedToAuthUserId.Value);

        if (Enum.TryParse<TicketStatus>(command.Status, true, out var status))
        {
            if (status == TicketStatus.Active && ticket.Status == TicketStatus.New) ticket.Activate();
            else if (status == TicketStatus.Closed && ticket.Status != TicketStatus.Closed) ticket.Close();
            else if (status == TicketStatus.Removed) ticket.Remove();
        }

        await _tickets.SaveChangesAsync(ct);
        return true;
    }
}

public sealed class DeleteTicketHandler : ICommandHandler<DeleteTicketCommand, bool>
{
    private readonly ITicketRepository _tickets;
    public DeleteTicketHandler(ITicketRepository tickets) => _tickets = tickets;

    public async Task<bool> Handle(DeleteTicketCommand command, CancellationToken ct)
    {
        var ticket = await _tickets.GetAsync(command.Id, ct);
        if (ticket is null) return false;
        ticket.Remove();
        await _tickets.SaveChangesAsync(ct);
        return true;
    }
}

public sealed class GetTicketsHandler : IQueryHandler<GetTicketsQuery, IReadOnlyList<TicketDto>>
{
    private readonly ITicketRepository _tickets;
    public GetTicketsHandler(ITicketRepository tickets) => _tickets = tickets;

    public async Task<IReadOnlyList<TicketDto>> Handle(GetTicketsQuery query, CancellationToken ct)
    {
        var items = await _tickets.ListAsync(TenantId.Create(query.TenantId), query.Status, query.Priority, ct);
        return items.Select(x => new TicketDto(x.Id, x.TenantId.Value, x.Subject, x.Description, x.RequestedByAccountId, x.ContactId, x.AssignedToAuthUserId, x.Status.ToString(), x.Priority.ToString(), x.DueDate)).ToList();
    }
}

public sealed class GetTicketByIdHandler : IQueryHandler<GetTicketByIdQuery, TicketDto?>
{
    private readonly ITicketRepository _tickets;
    public GetTicketByIdHandler(ITicketRepository tickets) => _tickets = tickets;

    public async Task<TicketDto?> Handle(GetTicketByIdQuery query, CancellationToken ct)
    {
        var x = await _tickets.GetAsync(query.Id, ct);
        return x is null || x.IsDeleted ? null : new TicketDto(x.Id, x.TenantId.Value, x.Subject, x.Description, x.RequestedByAccountId, x.ContactId, x.AssignedToAuthUserId, x.Status.ToString(), x.Priority.ToString(), x.DueDate);
    }
}

public sealed class CreateOpportunityHandler : ICommandHandler<CreateOpportunityCommand, int>
{
    private readonly IOpportunityRepository _opportunities;
    public CreateOpportunityHandler(IOpportunityRepository opportunities) => _opportunities = opportunities;

    public async Task<int> Handle(CreateOpportunityCommand command, CancellationToken ct)
    {
        var opportunity = Opportunity.Create(TenantId.Create(command.TenantId), command.Title, Money.Create(command.Value), command.AccountId, command.ExpectedCloseDate);
        if (command.ContactId is > 0) opportunity.LinkContact(command.ContactId.Value);
        if (Enum.TryParse<OpportunityStage>(command.Stage, true, out var stage) && stage != OpportunityStage.Prospecting)
            opportunity.MoveTo(stage);
        await _opportunities.AddAsync(opportunity, ct);
        await _opportunities.SaveChangesAsync(ct);
        return opportunity.Id;
    }
}

public sealed class UpdateOpportunityHandler : ICommandHandler<UpdateOpportunityCommand, bool>
{
    private readonly IOpportunityRepository _opportunities;
    public UpdateOpportunityHandler(IOpportunityRepository opportunities) => _opportunities = opportunities;

    public async Task<bool> Handle(UpdateOpportunityCommand command, CancellationToken ct)
    {
        var opportunity = await _opportunities.GetAsync(command.Id, ct);
        if (opportunity is null) return false;
        opportunity.Rename(command.Title);
        opportunity.ChangeValue(Money.Create(command.Value));
        if (command.ContactId is > 0) opportunity.LinkContact(command.ContactId.Value);
        if (Enum.TryParse<OpportunityStage>(command.Stage, true, out var stage) && stage != opportunity.Stage)
            opportunity.MoveTo(stage);
        await _opportunities.SaveChangesAsync(ct);
        return true;
    }
}

public sealed class DeleteOpportunityHandler : ICommandHandler<DeleteOpportunityCommand, bool>
{
    private readonly IOpportunityRepository _opportunities;
    public DeleteOpportunityHandler(IOpportunityRepository opportunities) => _opportunities = opportunities;

    public async Task<bool> Handle(DeleteOpportunityCommand command, CancellationToken ct)
    {
        var opportunity = await _opportunities.GetAsync(command.Id, ct);
        if (opportunity is null) return false;
        opportunity.Delete();
        await _opportunities.SaveChangesAsync(ct);
        return true;
    }
}

public sealed class GetOpportunitiesHandler : IQueryHandler<GetOpportunitiesQuery, IReadOnlyList<OpportunityDto>>
{
    private readonly IOpportunityRepository _opportunities;
    public GetOpportunitiesHandler(IOpportunityRepository opportunities) => _opportunities = opportunities;

    public async Task<IReadOnlyList<OpportunityDto>> Handle(GetOpportunitiesQuery query, CancellationToken ct)
    {
        var items = await _opportunities.ListAsync(TenantId.Create(query.TenantId), query.Stage, query.Search, ct);
        return items.Select(x => new OpportunityDto(x.Id, x.TenantId.Value, x.Title, x.Value.Amount, x.AccountId, x.ContactId, x.Stage.ToString(), x.ExpectedCloseDate)).ToList();
    }
}

public sealed class GetOpportunityByIdHandler : IQueryHandler<GetOpportunityByIdQuery, OpportunityDto?>
{
    private readonly IOpportunityRepository _opportunities;
    public GetOpportunityByIdHandler(IOpportunityRepository opportunities) => _opportunities = opportunities;

    public async Task<OpportunityDto?> Handle(GetOpportunityByIdQuery query, CancellationToken ct)
    {
        var x = await _opportunities.GetAsync(query.Id, ct);
        return x is null || x.IsDeleted ? null : new OpportunityDto(x.Id, x.TenantId.Value, x.Title, x.Value.Amount, x.AccountId, x.ContactId, x.Stage.ToString(), x.ExpectedCloseDate);
    }
}
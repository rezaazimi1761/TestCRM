using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Crm.Application.Commands;

public sealed record CreateAccountCommand(string TenantId, string Name, string? Industry, string? Website, string? Phone, string? Address) : ICommand<int>;
public sealed record UpdateAccountCommand(int Id, string Name, string? Industry, string? Website, string? Phone, string? Address) : ICommand<bool>;
public sealed record DeleteAccountCommand(int Id) : ICommand<bool>;

public sealed record CreateContactCommand(string TenantId, string FirstName, string LastName, string Email, string? Phone, string? JobTitle, int? AccountId) : ICommand<int>;
public sealed record UpdateContactCommand(int Id, string FirstName, string LastName, string Email, string? Phone, string? JobTitle, int? AccountId) : ICommand<bool>;
public sealed record DeleteContactCommand(int Id) : ICommand<bool>;

public sealed record CreateTicketCommand(string TenantId, string Subject, int AccountId, string Priority, DateTime? DueDate, string? Description, int? ContactId, int? AssignedToAuthUserId) : ICommand<int>;
public sealed record UpdateTicketCommand(int Id, string Subject, string Priority, string Status, DateTime? DueDate, string? Description, int? ContactId, int? AssignedToAuthUserId) : ICommand<bool>;
public sealed record DeleteTicketCommand(int Id) : ICommand<bool>;

public sealed record CreateOpportunityCommand(string TenantId, string Title, decimal Value, int? AccountId, int? ContactId, string Stage, DateTime? ExpectedCloseDate) : ICommand<int>;
public sealed record UpdateOpportunityCommand(int Id, string Title, decimal Value, int? ContactId, string Stage, DateTime? ExpectedCloseDate) : ICommand<bool>;
public sealed record DeleteOpportunityCommand(int Id) : ICommand<bool>;
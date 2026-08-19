using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Crm.Application.Commands;

public sealed record UpdateContactCommand(string TenantId, int Id, string FirstName, string LastName, string Email, string? Phone, string? JobTitle, int? AccountId) : ICommand<bool>;

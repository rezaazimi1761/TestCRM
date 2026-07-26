using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Crm.Application.Commands;

public sealed record CreateContactCommand(string TenantId, string FirstName, string LastName, string Email, string? Phone, string? JobTitle, int? AccountId) : ICommand<int>;

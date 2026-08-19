using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Crm.Application.Commands;

public sealed record DeleteOpportunityCommand(string TenantId, int Id) : ICommand<bool>;

using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Crm.Application.Commands;

public sealed record CreateOpportunityCommand(string TenantId, string Title, decimal Value, int? AccountId, int? ContactId, string Stage, DateTime? ExpectedCloseDate) : ICommand<int>;

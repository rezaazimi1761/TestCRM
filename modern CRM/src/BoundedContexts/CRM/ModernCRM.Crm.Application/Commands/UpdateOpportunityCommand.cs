using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Crm.Application.Commands;

public sealed record UpdateOpportunityCommand(string TenantId, int Id, string Title, decimal Value, int? ContactId, string Stage, DateTime? ExpectedCloseDate) : ICommand<bool>;

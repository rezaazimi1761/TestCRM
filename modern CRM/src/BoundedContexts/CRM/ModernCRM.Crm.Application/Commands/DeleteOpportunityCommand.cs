using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Crm.Application.Commands;

public sealed record DeleteOpportunityCommand(int Id) : ICommand<bool>;

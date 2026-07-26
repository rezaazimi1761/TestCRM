using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Auth.Application.Commands;

public sealed record UpdateTenantCommand(int Id, string DisplayName, Guid ServiceInstanceId, bool IsActive) : ICommand<bool>;

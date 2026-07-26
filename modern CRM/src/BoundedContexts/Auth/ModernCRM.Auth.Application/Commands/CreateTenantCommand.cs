using ModernCRM.SharedKernel.Application;

namespace ModernCRM.Auth.Application.Commands;

public sealed record CreateTenantCommand(string TenantId, string DisplayName, Guid ServiceInstanceId) : ICommand<int>;

using ModernCRM.Auth.Application.Commands;
using ModernCRM.Auth.Application.DTO;
using ModernCRM.Auth.Application.Queries;
using ModernCRM.Auth.Domain.Roles;
using ModernCRM.Auth.Domain.Tenants;
using ModernCRM.Auth.Domain.Users;
using ModernCRM.Auth.Domain.ValueObjects;
using ModernCRM.SharedKernel.Application;
using ModernCRM.SharedKernel.ValueObjects;

namespace ModernCRM.Auth.Application.Handlers;

public sealed class UpdateTenantHandler : ICommandHandler<UpdateTenantCommand, bool>
{
    private readonly ITenantRepository _tenants;
    public UpdateTenantHandler(ITenantRepository tenants) => _tenants = tenants;

    public async Task<bool> Handle(UpdateTenantCommand command, CancellationToken ct)
    {
        var tenant = await _tenants.GetByIdAsync(command.Id, ct);
        if (tenant is null) return false;
        tenant.Rename(command.DisplayName);
        tenant.MoveToServiceInstance(command.ServiceInstanceId);
        if (command.IsActive) tenant.Activate(); else tenant.Deactivate();
        await _tenants.UnitOfWork.SaveChangesAsync(ct);
        return true;
    }
}

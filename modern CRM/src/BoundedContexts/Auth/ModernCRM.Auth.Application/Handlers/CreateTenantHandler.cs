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

public sealed class CreateTenantHandler : ICommandHandler<CreateTenantCommand, int>
{
    private readonly ITenantRepository _tenants;
    public CreateTenantHandler(ITenantRepository tenants) => _tenants = tenants;

    public async Task<int> Handle(CreateTenantCommand command, CancellationToken ct)
    {
        var tenant = Tenant.Create(TenantId.Create(command.TenantId), command.DisplayName, command.ServiceInstanceId);
        await _tenants.AddAsync(tenant, ct);
        await _tenants.SaveChangesAsync(ct);
        return tenant.Id;
    }
}

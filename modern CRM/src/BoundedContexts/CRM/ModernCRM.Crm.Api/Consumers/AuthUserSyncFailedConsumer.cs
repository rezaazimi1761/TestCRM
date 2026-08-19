using MassTransit;
using ModernCRM.SharedKernel.IntegrationEvents;

using ModernCRM.Crm.Api.UserSync;
using ModernCRM.Crm.Application.Frontend;

namespace ModernCRM.Crm.Api.Consumers;

public sealed class AuthUserSyncFailedConsumer(ICrmFrontendRepository persistence) : IConsumer<AuthUserSyncFailed>
{
    public async Task Consume(ConsumeContext<AuthUserSyncFailed> context)
    {
        var user = await persistence.FindUserForSyncAsync(context.Message.CrmUserId, context.CancellationToken);
        if (user is null) return;
        user.SyncStatus = "Failed";
        user.SyncError = context.Message.Error;
        user.UpdatedAt = DateTime.UtcNow;
        await persistence.UnitOfWork.SaveChangesAsync(context.CancellationToken);
    }
}

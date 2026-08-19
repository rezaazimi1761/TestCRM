using MassTransit;
using ModernCRM.SharedKernel.IntegrationEvents;

using ModernCRM.Crm.Api.UserSync;
using ModernCRM.Crm.Application.Frontend;

namespace ModernCRM.Crm.Api.Consumers;

public sealed class AuthUserSyncedConsumer(ICrmFrontendRepository persistence) : IConsumer<AuthUserSynced>
{
    public async Task Consume(ConsumeContext<AuthUserSynced> context)
    {
        var user = await persistence.FindUserForSyncAsync(context.Message.CrmUserId, context.CancellationToken);
        if (user is null) return;
        user.AuthUserId = context.Message.AuthUserId;
        user.SyncStatus = "Synced";
        user.SyncError = null;
        user.UpdatedAt = DateTime.UtcNow;
        await persistence.UnitOfWork.SaveChangesAsync(context.CancellationToken);
    }
}

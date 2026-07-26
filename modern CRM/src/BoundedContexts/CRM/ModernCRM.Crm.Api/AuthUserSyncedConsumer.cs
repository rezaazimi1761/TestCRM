using MassTransit;
using Microsoft.EntityFrameworkCore;
using ModernCRM.SharedKernel.IntegrationEvents;

namespace ModernCRM.Crm.Api.UserSync;

public sealed class AuthUserSyncedConsumer(CrmIntegrationDbContext db) : IConsumer<AuthUserSynced>
{
    public async Task Consume(ConsumeContext<AuthUserSynced> context)
    {
        var user = await db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == context.Message.CrmUserId, context.CancellationToken);
        if (user is null) return;
        user.AuthUserId = context.Message.AuthUserId;
        user.SyncStatus = "Synced";
        user.SyncError = null;
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(context.CancellationToken);
    }
}

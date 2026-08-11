using MassTransit;
using Microsoft.EntityFrameworkCore;
using ModernCRM.Auth.Application.Handlers;
using ModernCRM.SharedKernel.IntegrationEvents;

using ModernCRM.Auth.Api.UserSync;

namespace ModernCRM.Auth.Api.Consumers;

public sealed class SyncUserToAuthConsumer(AuthIntegrationDbContext db, IPasswordHasher hasher) : IConsumer<SyncUserToAuth>
{
    public async Task Consume(ConsumeContext<SyncUserToAuth> context)
    {
        var m=context.Message;
        try
        {
            var user=await db.Users.FirstOrDefaultAsync(x=>x.CrmUserId==m.CrmUserId&&x.TenantId==m.TenantId,context.CancellationToken);
            switch(m.Operation)
            {
                case UserIntegrationOperation.Created:
                    if(user is not null)throw new InvalidOperationException("CRM user is already provisioned in Auth.");
                    if(string.IsNullOrWhiteSpace(m.Password))throw new InvalidOperationException("Password is required for user provisioning.");
                    var authUserId=100000+m.CrmUserId;
                    user=new SyncedAuthUser{Id=authUserId,CrmUserId=m.CrmUserId,TenantId=m.TenantId,Username=m.Username,Email=m.Email,FirstName=m.FirstName,LastName=m.LastName,PasswordHash=hasher.Hash(m.Password),Role=m.Role,IsActive=true,CreatedAt=DateTime.UtcNow};
                    db.Users.Add(user);
                    break;
                case UserIntegrationOperation.Updated:
                    if(user is null)throw new InvalidOperationException("Auth user was not found for update.");
                    user.Email=m.Email;user.FirstName=m.FirstName;user.LastName=m.LastName;user.Role=m.Role;user.IsActive=m.IsActive;user.IsDeleted=false;user.UpdatedAt=DateTime.UtcNow;
                    break;
                case UserIntegrationOperation.Deleted:
                    if(user is null)throw new InvalidOperationException("Auth user was not found for delete.");
                    user.IsActive=false;user.IsDeleted=true;user.UpdatedAt=DateTime.UtcNow;
                    break;
                default:throw new ArgumentOutOfRangeException(nameof(m.Operation));
            }
            await db.SaveChangesAsync(context.CancellationToken);
            await context.Publish(new AuthUserSynced(m.CorrelationId,m.Operation,m.CrmUserId,user.Id,m.TenantId,DateTime.UtcNow));
        }
        catch(Exception ex)
        {
            await context.Publish(new AuthUserSyncFailed(m.CorrelationId,m.CrmUserId,m.TenantId,ex.Message,DateTime.UtcNow));
        }
    }
}

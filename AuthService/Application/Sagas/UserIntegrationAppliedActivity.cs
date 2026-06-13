using AuthService.Domain.Entities;
using AuthService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Events;

namespace AuthService.Application.Sagas;

public class UserIntegrationAppliedActivity :
    IStateMachineActivity<UserIntegrationSagaState, UserIntegrationAppliedEvent>
{
    private readonly AuthDbContext _db;

    public UserIntegrationAppliedActivity(AuthDbContext db) => _db = db;

    public void Probe(ProbeContext context) => context.CreateScope("user-integration-applied");

    public void Accept(StateMachineVisitor visitor) => visitor.Visit(this);

    public async Task Execute(
        BehaviorContext<UserIntegrationSagaState, UserIntegrationAppliedEvent> context,
        IBehavior<UserIntegrationSagaState, UserIntegrationAppliedEvent> next)
    {
        var msg = context.Message;
        var user = await _db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == msg.AuthUserId && u.TenantId == msg.TenantId,
                context.CancellationToken);

        if (user is not null)
        {
            user.IntegrationStatus = UserIntegrationStatus.Synced;
            user.IntegrationError = null;
            if (msg.Operation != UserIntegrationOperation.Deleted)
            {
                user.IsDeleted = false;
            }
            await _db.SaveChangesAsync(context.CancellationToken);
        }

        await next.Execute(context);
    }

    public Task Faulted<TException>(
        BehaviorExceptionContext<UserIntegrationSagaState, UserIntegrationAppliedEvent, TException> context,
        IBehavior<UserIntegrationSagaState, UserIntegrationAppliedEvent> next)
        where TException : Exception
        => next.Faulted(context);
}

using AuthService.Domain.Entities;
using AuthService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Events;

namespace AuthService.Application.Sagas;

public class UserIntegrationFaultActivity :
    IStateMachineActivity<UserIntegrationSagaState, Fault<UserIntegrationEvent>>
{
    private readonly AuthDbContext _db;
    private readonly ILogger<UserIntegrationFaultActivity> _logger;

    public UserIntegrationFaultActivity(AuthDbContext db, ILogger<UserIntegrationFaultActivity> logger)
    {
        _db = db;
        _logger = logger;
    }

    public void Probe(ProbeContext context) => context.CreateScope("user-integration-fault");

    public void Accept(StateMachineVisitor visitor) => visitor.Visit(this);

    public async Task Execute(
        BehaviorContext<UserIntegrationSagaState, Fault<UserIntegrationEvent>> context,
        IBehavior<UserIntegrationSagaState, Fault<UserIntegrationEvent>> next)
    {
        var evt = context.Message.Message;
        var error = context.Message.Exceptions.FirstOrDefault()?.Message
            ?? "CRM integration failed.";

        var user = await _db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == evt.AuthUserId && u.TenantId == evt.TenantId,
                context.CancellationToken);

        if (user is not null)
        {
            user.IsActive = false;
            user.IsDeleted = true;
            user.IntegrationStatus = UserIntegrationStatus.Failed;
            user.IntegrationError = error;
            await _db.SaveChangesAsync(context.CancellationToken);
        }

        _logger.LogError(
            "CRM user integration failed. authUserId={AuthUserId} tenant={TenantId} operation={Operation} error={Error}",
            evt.AuthUserId, evt.TenantId, evt.Operation, error);

        await next.Execute(context);
    }

    public Task Faulted<TException>(
        BehaviorExceptionContext<UserIntegrationSagaState, Fault<UserIntegrationEvent>, TException> context,
        IBehavior<UserIntegrationSagaState, Fault<UserIntegrationEvent>> next)
        where TException : Exception
        => next.Faulted(context);
}

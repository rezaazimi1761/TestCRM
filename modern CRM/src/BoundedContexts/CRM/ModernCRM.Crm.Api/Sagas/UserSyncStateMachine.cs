using MassTransit;
using ModernCRM.SharedKernel.IntegrationEvents;
using ModernCRM.Crm.Infrastructure.Integration;

namespace ModernCRM.Crm.Api.UserSync;

public sealed class UserSyncStateMachine : MassTransitStateMachine<UserSyncSagaState>
{
    public State Pending { get; private set; } = null!;
    public State Failed { get; private set; } = null!;
    public Event<CrmUserSyncRequested> Requested { get; private set; } = null!;
    public Event<AuthUserSynced> Synced { get; private set; } = null!;
    public Event<AuthUserSyncFailed> SyncFailed { get; private set; } = null!;
    public Event<Fault<SyncUserToAuth>> CommandFaulted { get; private set; } = null!;

    public UserSyncStateMachine()
    {
        InstanceState(x => x.CurrentState);
        Event(() => Requested, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => Synced, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => SyncFailed, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => CommandFaulted, x => x.CorrelateById(m => m.Message.Message.CorrelationId));

        Initially(When(Requested)
            .Then(x =>
            {
                x.Saga.CrmUserId = x.Message.CrmUserId;
                x.Saga.TenantId = x.Message.TenantId;
                x.Saga.Operation = x.Message.Operation.ToString();
                x.Saga.StartedAt = DateTime.UtcNow;
            })
            .Publish(x => new SyncUserToAuth(x.Message.CorrelationId, x.Message.Operation, x.Message.CrmUserId, x.Message.AuthUserId, x.Message.TenantId, x.Message.Username, x.Message.Email, x.Message.FirstName, x.Message.LastName, x.Message.Role, x.Message.IsActive, x.Message.Password))
            .TransitionTo(Pending));

        During(Pending,
            When(Synced).Then(x => x.Saga.CompletedAt = x.Message.SyncedAtUtc).Finalize(),
            When(SyncFailed).Then(x => { x.Saga.Error = x.Message.Error; x.Saga.CompletedAt = x.Message.FailedAtUtc; }).TransitionTo(Failed),
            When(CommandFaulted).Then(x => { x.Saga.Error = x.Message.Exceptions.FirstOrDefault()?.Message ?? "Auth synchronization failed."; x.Saga.CompletedAt = DateTime.UtcNow; }).TransitionTo(Failed));
        SetCompletedWhenFinalized();
    }
}

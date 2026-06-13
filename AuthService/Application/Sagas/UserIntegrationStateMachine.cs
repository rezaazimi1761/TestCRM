using MassTransit;
using Shared.Contracts.Events;

namespace AuthService.Application.Sagas;

public class UserIntegrationStateMachine : MassTransitStateMachine<UserIntegrationSagaState>
{
    public State Pending { get; private set; } = null!;
    public State Failed { get; private set; } = null!;

    public Event<UserIntegrationEvent> Requested { get; private set; } = null!;
    public Event<UserIntegrationAppliedEvent> Applied { get; private set; } = null!;
    public Event<Fault<UserIntegrationEvent>> IntegrationFaulted { get; private set; } = null!;

    public UserIntegrationStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => Requested, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => Applied, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => IntegrationFaulted, x => x.CorrelateById(m => m.Message.Message.CorrelationId));

        Initially(
            When(Requested)
                .Then(ctx =>
                {
                    ctx.Saga.AuthUserId = ctx.Message.AuthUserId;
                    ctx.Saga.TenantId   = ctx.Message.TenantId;
                    ctx.Saga.Username   = ctx.Message.Username;
                    ctx.Saga.Operation  = ctx.Message.Operation.ToString();
                    ctx.Saga.CreatedAt  = DateTime.UtcNow;
                })
                .TransitionTo(Pending));

        During(Pending,
            When(Applied)
                .Then(ctx => ctx.Saga.AppliedAt = ctx.Message.AppliedAt)
                .Activity(x => x.OfType<UserIntegrationAppliedActivity>())
                .Finalize(),
            When(IntegrationFaulted)
                .Then(ctx =>
                {
                    ctx.Saga.FaultedAt = DateTime.UtcNow;
                    ctx.Saga.FaultReason = ctx.Message.Exceptions.FirstOrDefault()?.Message
                        ?? "CRM integration failed.";
                })
                .Activity(x => x.OfType<UserIntegrationFaultActivity>())
                .TransitionTo(Failed));

        SetCompletedWhenFinalized();
    }
}

using ModernCRM.SharedKernel.IntegrationEvents;
namespace ModernCRM.Crm.Infrastructure.Messaging;
[Obsolete("User synchronization results are consumed by ModernCRM.Crm.Api through MassTransit.")]
public sealed class UserIntegrationConsumer
{
    public Task ConsumeAsync(AuthUserSynced integrationEvent, CancellationToken ct) => Task.CompletedTask;
}
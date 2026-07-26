using ModernCRM.SharedKernel.IntegrationEvents;

namespace ModernCRM.Crm.Infrastructure.Messaging;

public sealed class UserIntegrationConsumer
{
    public Task ConsumeAsync(UserIntegrationEvent integrationEvent, CancellationToken ct)
    {
        // ACL: translate Auth user language into CRM UserReference model.
        return Task.CompletedTask;
    }
}

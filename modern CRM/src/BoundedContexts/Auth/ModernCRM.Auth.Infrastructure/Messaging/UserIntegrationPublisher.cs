using ModernCRM.SharedKernel.IntegrationEvents;

namespace ModernCRM.Auth.Infrastructure.Messaging;

public interface IUserIntegrationPublisher { Task PublishAsync(UserIntegrationEvent integrationEvent, CancellationToken ct); }
public sealed class UserIntegrationPublisher : IUserIntegrationPublisher
{
    public Task PublishAsync(UserIntegrationEvent integrationEvent, CancellationToken ct) => Task.CompletedTask;
}

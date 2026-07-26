using ModernCRM.SharedKernel.IntegrationEvents;
namespace ModernCRM.Auth.Infrastructure.Messaging;
[Obsolete("User synchronization is initiated by CRM and consumed by ModernCRM.Auth.Api.")]
public interface IUserIntegrationPublisher { Task PublishAsync(AuthUserSynced integrationEvent, CancellationToken ct); }
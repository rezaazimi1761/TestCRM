using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Auth.Domain.Users;

public sealed record AuthUserUpdatedDomainEvent(int UserId, string TenantId, string Username) : DomainEvent;

using ModernCRM.SharedKernel.BuildingBlocks;

namespace ModernCRM.Auth.Domain.Users;

public sealed record AuthUserCreatedDomainEvent(int UserId, string TenantId, string Username) : DomainEvent;
public sealed record AuthUserUpdatedDomainEvent(int UserId, string TenantId, string Username) : DomainEvent;
public sealed record AuthUserDeletedDomainEvent(int UserId, string TenantId, string Username) : DomainEvent;

namespace ModernCRM.Crm.Infrastructure.Identity;

public interface ICurrentTenantProvider { string TenantId { get; } }
public sealed class CurrentTenantProvider : ICurrentTenantProvider { public string TenantId => "default"; }

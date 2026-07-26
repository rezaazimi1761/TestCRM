using Microsoft.AspNetCore.Http;

namespace ModernCRM.Crm.Infrastructure.Identity;

public interface ICurrentTenantProvider { string TenantId { get; } }

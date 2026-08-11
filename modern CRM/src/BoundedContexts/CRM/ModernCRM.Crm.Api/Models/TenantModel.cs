using System.Reflection;

namespace ModernCRM.Crm.Api.Frontend;

public abstract class TenantModel
{
    public int Id { get; set; }
    public string TenantId { get; set; } = "default";
    public bool IsDeleted { get; set; }
}

using System.Reflection;

namespace ModernCRM.Crm.Api.Frontend;

public sealed class AccountModel : TenantModel
{
    public string? Name { get; set; }
    public string? Industry { get; set; }
    public string? Website { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
}

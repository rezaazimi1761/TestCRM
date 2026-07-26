using System.Reflection;

namespace ModernCRM.Crm.Api.Frontend;

public sealed class ContactModel : TenantModel
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? JobTitle { get; set; }
    public string? Notes { get; set; }
    public int? AccountId { get; set; }
}

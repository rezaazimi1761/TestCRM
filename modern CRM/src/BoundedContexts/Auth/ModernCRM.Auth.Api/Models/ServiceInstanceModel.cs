namespace ModernCRM.Auth.Api.Services;

public sealed class ServiceInstanceModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string ApiUrl { get; set; } = "";
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TenantCount { get; set; }
}

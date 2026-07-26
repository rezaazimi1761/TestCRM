namespace ModernCRM.Auth.Api.Services;

public sealed class ServiceInstanceStore(IConfiguration configuration)
{
    public object SyncRoot { get; } = new();
    public List<ServiceInstanceModel> Items { get; } = new()
    {
        new ServiceInstanceModel
        {
            Id = configuration.GetValue<Guid>("Seed:DefaultServiceInstanceId"),
            Name = configuration["Seed:DefaultServiceInstanceName"] ?? "crm-local",
            ApiUrl = configuration["Seed:DefaultServiceInstanceUrl"] ?? "http://localhost:9040",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }
    };
}
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
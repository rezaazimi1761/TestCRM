using ModernCRM.Crm.Application.Frontend;

namespace ModernCRM.Crm.Infrastructure.Integration;

public sealed class FrontendCrudService(ICrmFrontendRepository repository) : ILeadService, IActivityService
{
    public Task<PagedResult<LeadModel>> GetPageAsync(
        string tenantId, int page, int pageSize, string? sortBy, bool sortDesc,
        string? search, string? status, CancellationToken cancellationToken)
        => repository.PageLeadsAsync(tenantId, page, pageSize, sortBy, sortDesc, search, status, cancellationToken);

    public Task<LeadModel?> GetAsync(string tenantId, int id, CancellationToken cancellationToken)
        => repository.FindLeadAsync(tenantId, id, cancellationToken);

    public async Task<int> CreateAsync(string tenantId, LeadInput input, CancellationToken cancellationToken)
    {
        var item = new LeadModel
        {
            TenantId = tenantId,
            FirstName = input.FirstName,
            LastName = input.LastName,
            Email = input.Email,
            Phone = input.Phone,
            Company = input.Company,
            Status = input.Status ?? "New",
            Source = input.Source,
            Notes = input.Notes
        };
        repository.Add(item);
        await repository.UnitOfWork.SaveChangesAsync(cancellationToken);
        return item.Id;
    }

    public async Task<bool> UpdateAsync(string tenantId, int id, LeadInput input, CancellationToken cancellationToken)
    {
        var item = await repository.FindLeadAsync(tenantId, id, cancellationToken);
        if (item is null) return false;
        item.FirstName = input.FirstName;
        item.LastName = input.LastName;
        item.Email = input.Email;
        item.Phone = input.Phone;
        item.Company = input.Company;
        item.Status = input.Status ?? item.Status;
        item.Source = input.Source;
        item.Notes = input.Notes;
        await repository.UnitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    async Task<bool> ILeadService.DeleteAsync(string tenantId, int id, CancellationToken cancellationToken)
    {
        var item = await repository.FindLeadAsync(tenantId, id, cancellationToken);
        if (item is null) return false;
        item.IsDeleted = true;
        await repository.UnitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    Task<PagedResult<ActivityModel>> IActivityService.GetPageAsync(
        string tenantId, int page, int pageSize, string? sortBy, bool sortDesc,
        string? search, string? type, bool? isCompleted, CancellationToken cancellationToken)
        => repository.PageActivitiesAsync(tenantId, page, pageSize, sortBy, sortDesc, search, type, isCompleted, cancellationToken);

    Task<ActivityModel?> IActivityService.GetAsync(string tenantId, int id, CancellationToken cancellationToken)
        => repository.FindActivityAsync(tenantId, id, cancellationToken);

    async Task<int> IActivityService.CreateAsync(string tenantId, ActivityInput input, CancellationToken cancellationToken)
    {
        var item = new ActivityModel
        {
            TenantId = tenantId,
            Subject = input.Subject,
            Type = input.Type ?? "Task",
            Description = input.Description,
            ContactId = input.ContactId,
            DueDate = input.DueDate,
            IsCompleted = input.IsCompleted ?? false
        };
        item.ContactName = await repository.GetContactNameAsync(tenantId, item.ContactId, cancellationToken);
        repository.Add(item);
        await repository.UnitOfWork.SaveChangesAsync(cancellationToken);
        return item.Id;
    }

    async Task<bool> IActivityService.UpdateAsync(string tenantId, int id, ActivityInput input, CancellationToken cancellationToken)
    {
        var item = await repository.FindActivityAsync(tenantId, id, cancellationToken);
        if (item is null) return false;
        item.Subject = input.Subject;
        item.Type = input.Type ?? item.Type;
        item.Description = input.Description;
        item.ContactId = input.ContactId;
        item.DueDate = input.DueDate;
        item.IsCompleted = input.IsCompleted ?? item.IsCompleted;
        item.ContactName = await repository.GetContactNameAsync(tenantId, item.ContactId, cancellationToken);
        await repository.UnitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    async Task<bool> IActivityService.DeleteAsync(string tenantId, int id, CancellationToken cancellationToken)
    {
        var item = await repository.FindActivityAsync(tenantId, id, cancellationToken);
        if (item is null) return false;
        item.IsDeleted = true;
        await repository.UnitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}


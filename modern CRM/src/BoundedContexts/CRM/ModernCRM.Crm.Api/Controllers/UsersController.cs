using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModernCRM.Crm.Api.UserSync;
using ModernCRM.SharedKernel.IntegrationEvents;

namespace ModernCRM.Crm.Api.Controllers;

[ApiController, Route("api/users")]
public sealed class UsersController(CrmIntegrationDbContext db, IPublishEndpoint publisher) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(int page=1,int pageSize=20,string? sortBy=null,bool sortDesc=false,string? search=null,string? role=null,CancellationToken ct=default)
    {
        page=Math.Max(1,page);pageSize=Math.Clamp(pageSize<1?20:pageSize,1,500);var tenant=Tenant();
        var q=db.Users.AsNoTracking().Where(x=>x.TenantId==tenant&&!x.IsDeleted);
        if(!string.IsNullOrWhiteSpace(search))q=q.Where(x=>x.Username.Contains(search)||x.FirstName.Contains(search)||x.LastName.Contains(search)||x.Email.Contains(search));
        if(!string.IsNullOrWhiteSpace(role))q=q.Where(x=>x.Role==role);
        q=sortBy switch{"username"=>sortDesc?q.OrderByDescending(x=>x.Username):q.OrderBy(x=>x.Username),"firstname"=>sortDesc?q.OrderByDescending(x=>x.FirstName):q.OrderBy(x=>x.FirstName),"lastname"=>sortDesc?q.OrderByDescending(x=>x.LastName):q.OrderBy(x=>x.LastName),"email"=>sortDesc?q.OrderByDescending(x=>x.Email):q.OrderBy(x=>x.Email),"role"=>sortDesc?q.OrderByDescending(x=>x.Role):q.OrderBy(x=>x.Role),_=>q.OrderByDescending(x=>x.Id)};
        var total=await q.CountAsync(ct);var users=await q.Skip((page-1)*pageSize).Take(pageSize).ToListAsync(ct);
        return Ok(new PagedResult<UserView>(users.Select(ToView).ToList(),total,page,pageSize));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id,CancellationToken ct){var x=await db.Users.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==id&&x.TenantId==Tenant()&&!x.IsDeleted,ct);return x is null?NotFound():Ok(ToView(x));}

    [HttpPost]
    public async Task<IActionResult> Create(CreatePayload r,CancellationToken ct)
    {
        if(string.IsNullOrWhiteSpace(r.Username)||string.IsNullOrWhiteSpace(r.Email)||string.IsNullOrWhiteSpace(r.FirstName)||string.IsNullOrWhiteSpace(r.LastName)||string.IsNullOrWhiteSpace(r.Password))return BadRequest(new{message="Username, email, name and password are required."});
        var tenant=Tenant();if(await db.Users.AnyAsync(x=>x.TenantId==tenant&&(x.Username==r.Username||x.Email==r.Email)&&!x.IsDeleted,ct))return Conflict(new{message="Username or email already exists in CRM."});
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        var user=new CrmUser{TenantId=tenant,Username=r.Username,Email=r.Email,FirstName=r.FirstName,LastName=r.LastName,Role=r.Role??"User",IsActive=true,SyncStatus="Pending",CreatedAt=DateTime.UtcNow};
        db.Users.Add(user);await db.SaveChangesAsync(ct);
        await publisher.Publish(new CrmUserSyncRequested(Guid.NewGuid(),UserIntegrationOperation.Created,user.Id,null,tenant,user.Username,user.Email,user.FirstName,user.LastName,user.Role,user.IsActive,r.Password),ct);
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
        return CreatedAtAction(nameof(Get),new{id=user.Id},user.Id);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id,UpdatePayload r,CancellationToken ct)
    {
        var user=await db.Users.FirstOrDefaultAsync(x=>x.Id==id&&x.TenantId==Tenant()&&!x.IsDeleted,ct);if(user is null)return NotFound();
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        user.FirstName=r.FirstName??user.FirstName;user.LastName=r.LastName??user.LastName;user.Email=r.Email??user.Email;user.Role=r.Role??user.Role;user.IsActive=r.IsActive;user.SyncStatus="Pending";user.SyncError=null;user.UpdatedAt=DateTime.UtcNow;
        await publisher.Publish(new CrmUserSyncRequested(Guid.NewGuid(),UserIntegrationOperation.Updated,user.Id,user.AuthUserId,user.TenantId,user.Username,user.Email,user.FirstName,user.LastName,user.Role,user.IsActive,null),ct);
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id,CancellationToken ct)
    {
        var user=await db.Users.FirstOrDefaultAsync(x=>x.Id==id&&x.TenantId==Tenant()&&!x.IsDeleted,ct);if(user is null)return NotFound();
        await using var tx=await db.Database.BeginTransactionAsync(ct);user.IsDeleted=true;user.IsActive=false;user.SyncStatus="Pending";user.SyncError=null;user.UpdatedAt=DateTime.UtcNow;
        await publisher.Publish(new CrmUserSyncRequested(Guid.NewGuid(),UserIntegrationOperation.Deleted,user.Id,user.AuthUserId,user.TenantId,user.Username,user.Email,user.FirstName,user.LastName,user.Role,false,null),ct);
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return NoContent();
    }

    private string Tenant()=>User.FindFirst("tenant_id")?.Value??"default";
    private static UserView ToView(CrmUser x)=>new(x.Id,x.AuthUserId,x.TenantId,x.Username,x.FirstName,x.LastName,x.Email,x.Role,x.IsActive,x.IsDeleted,x.SyncStatus,x.SyncError,x.CreatedAt);
    public sealed record CreatePayload(string? Username,string? Email,string? FirstName,string? LastName,string? Password,string? Role);
    public sealed record UpdatePayload(string? Email,string? FirstName,string? LastName,string? Role,bool IsActive);
    public sealed record UserView(int Id,int? AuthUserId,string TenantId,string Username,string FirstName,string LastName,string Email,string Role,bool IsActive,bool IsDeleted,string IntegrationStatus,string? IntegrationError,DateTime CreatedAt);
    public sealed record PagedResult<T>(IReadOnlyList<T> Items,int TotalCount,int Page,int PageSize);
}
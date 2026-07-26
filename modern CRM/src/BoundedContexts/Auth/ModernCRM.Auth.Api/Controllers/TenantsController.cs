using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernCRM.Auth.Api.Services;
using ModernCRM.Auth.Domain.Tenants;
using ModernCRM.Auth.Infrastructure.Persistence;
using ModernCRM.SharedKernel.ValueObjects;

namespace ModernCRM.Auth.Api.Controllers;

[ApiController,Authorize,Route("api/tenants")]
public sealed class TenantsController(AuthDbContext db,ServiceInstanceStore instances):ControllerBase
{
 [HttpGet] public IActionResult GetAll(){lock(instances.SyncRoot){var result=db.Tenants.Where(t=>!t.IsDeleted).Select(t=>{var i=instances.Items.FirstOrDefault(i=>i.Id==t.ServiceInstanceId);return new TenantView(t.Id,t.TenantId.Value,t.DisplayName,null,t.IsActive,DateTime.UtcNow,db.Users.Count(u=>u.TenantId==t.TenantId&&!u.IsDeleted),t.ServiceInstanceId,i?.Name,i?.ApiUrl);}).ToList();return Ok(result);}}
 [HttpGet("{slug}")] public IActionResult Get(string slug){var t=db.Tenants.FirstOrDefault(x=>x.TenantId.Value==slug&&!x.IsDeleted);return t is null?NotFound():Ok(t);}
 [HttpPost] public async Task<IActionResult> Create(TenantPayload r,CancellationToken ct){if(string.IsNullOrWhiteSpace(r.Slug)||string.IsNullOrWhiteSpace(r.DisplayName)||r.ServiceInstanceId==Guid.Empty)return BadRequest(new{message="Slug, display name and service instance are required."});if(db.Tenants.Any(x=>x.TenantId.Value==r.Slug&&!x.IsDeleted))return Conflict(new{message="Tenant already exists."});var t=Tenant.Create(TenantId.Create(r.Slug),r.DisplayName,r.ServiceInstanceId);t.GetType().GetProperty("Id")!.SetValue(t,db.NextTenantId());db.Tenants.Add(t);await db.SaveChangesAsync(ct);return CreatedAtAction(nameof(Get),new{slug=t.TenantId.Value},t.Id);}
 [HttpPut("{slug}")] public async Task<IActionResult> Update(string slug,TenantPayload r,CancellationToken ct){var t=db.Tenants.FirstOrDefault(x=>x.TenantId.Value==slug&&!x.IsDeleted);if(t is null)return NotFound();if(!string.IsNullOrWhiteSpace(r.DisplayName))t.Rename(r.DisplayName);if(r.ServiceInstanceId!=Guid.Empty)t.MoveToServiceInstance(r.ServiceInstanceId);if(r.IsActive==true)t.Activate();else if(r.IsActive==false)t.Deactivate();await db.SaveChangesAsync(ct);return NoContent();}
 [HttpPost("{slug}/activate")] public async Task<IActionResult> Activate(string slug,CancellationToken ct){var t=db.Tenants.FirstOrDefault(x=>x.TenantId.Value==slug&&!x.IsDeleted);if(t is null)return NotFound();t.Activate();await db.SaveChangesAsync(ct);return NoContent();}
 [HttpPost("{slug}/deactivate")] public async Task<IActionResult> Deactivate(string slug,CancellationToken ct){var t=db.Tenants.FirstOrDefault(x=>x.TenantId.Value==slug&&!x.IsDeleted);if(t is null)return NotFound();t.Deactivate();await db.SaveChangesAsync(ct);return NoContent();}
 [HttpDelete("{slug}")] public async Task<IActionResult> Delete(string slug,CancellationToken ct){var t=db.Tenants.FirstOrDefault(x=>x.TenantId.Value==slug&&!x.IsDeleted);if(t is null)return NotFound();t.Delete();await db.SaveChangesAsync(ct);return NoContent();}
}
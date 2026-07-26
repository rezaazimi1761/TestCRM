using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernCRM.Auth.Api.Services;
using ModernCRM.Auth.Infrastructure.Persistence;

namespace ModernCRM.Auth.Api.Controllers;

[ApiController,Authorize(Roles="SuperUser"),Route("api/service-instances")]
public sealed class ServiceInstancesController(ServiceInstanceStore store,AuthDbContext db):ControllerBase
{
 [HttpGet] public IActionResult GetAll(){lock(store.SyncRoot){foreach(var i in store.Items)i.TenantCount=db.Tenants.Count(t=>t.ServiceInstanceId==i.Id&&!t.IsDeleted);return Ok(store.Items.ToList());}}
 [HttpGet("{id:guid}")] public IActionResult Get(Guid id){lock(store.SyncRoot){var x=store.Items.FirstOrDefault(x=>x.Id==id);return x is null?NotFound():Ok(x);}}
 [HttpPost] public IActionResult Create(ServiceInstancePayload r){if(string.IsNullOrWhiteSpace(r.Name)||string.IsNullOrWhiteSpace(r.ApiUrl))return BadRequest(new{message="Name and API URL are required."});lock(store.SyncRoot){var x=new ServiceInstanceModel{Id=Guid.NewGuid(),Name=r.Name,ApiUrl=r.ApiUrl,Description=r.Description,IsActive=true,CreatedAt=DateTime.UtcNow};store.Items.Add(x);return CreatedAtAction(nameof(Get),new{id=x.Id},x.Id);}}
 [HttpPut("{id:guid}")] public IActionResult Update(Guid id,ServiceInstancePayload r){lock(store.SyncRoot){var x=store.Items.FirstOrDefault(x=>x.Id==id);if(x is null)return NotFound();x.Name=r.Name??x.Name;x.ApiUrl=r.ApiUrl??x.ApiUrl;x.Description=r.Description;x.IsActive=r.IsActive??x.IsActive;return NoContent();}}
 [HttpDelete("{id:guid}")] public IActionResult Delete(Guid id){lock(store.SyncRoot){if(db.Tenants.Any(t=>t.ServiceInstanceId==id&&!t.IsDeleted))return Conflict(new{message="Service instance has attached tenants."});var x=store.Items.FirstOrDefault(x=>x.Id==id);if(x is null)return NotFound();store.Items.Remove(x);return NoContent();}}
}
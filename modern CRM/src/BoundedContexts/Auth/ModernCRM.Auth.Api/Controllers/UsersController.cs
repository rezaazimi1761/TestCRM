using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernCRM.Auth.Application.Commands;
using ModernCRM.Auth.Application.Handlers;
using ModernCRM.Auth.Infrastructure.Persistence;

namespace ModernCRM.Auth.Api.Controllers;

[ApiController,Authorize,Route("api/users")]
public sealed class UsersController(AuthDbContext db):ControllerBase
{
 [HttpGet]
 public IActionResult GetAll(int page=1,int pageSize=20,string? sortBy=null,bool sortDesc=false,string? search=null,string? role=null)
 {
  page=Math.Max(page,1);pageSize=Math.Clamp(pageSize<1?20:pageSize,1,500);
  var tenant=User.FindFirst("tenant_id")?.Value??"default";
  IEnumerable<ModernCRM.Auth.Domain.Users.AuthUser> q=db.Users.Where(x=>x.TenantId.Value==tenant&&!x.IsDeleted);
  if(!string.IsNullOrWhiteSpace(search))q=q.Where(x=>x.Username.Value.Contains(search,StringComparison.OrdinalIgnoreCase)||x.FirstName.Contains(search,StringComparison.OrdinalIgnoreCase)||x.LastName.Contains(search,StringComparison.OrdinalIgnoreCase)||x.Email.Value.Contains(search,StringComparison.OrdinalIgnoreCase));
  if(!string.IsNullOrWhiteSpace(role))q=q.Where(x=>x.Role.ToString().Equals(role,StringComparison.OrdinalIgnoreCase));
  q=sortBy?.ToLowerInvariant() switch{"username"=>sortDesc?q.OrderByDescending(x=>x.Username.Value):q.OrderBy(x=>x.Username.Value),"firstname"=>sortDesc?q.OrderByDescending(x=>x.FirstName):q.OrderBy(x=>x.FirstName),"lastname"=>sortDesc?q.OrderByDescending(x=>x.LastName):q.OrderBy(x=>x.LastName),"email"=>sortDesc?q.OrderByDescending(x=>x.Email.Value):q.OrderBy(x=>x.Email.Value),"role"=>sortDesc?q.OrderByDescending(x=>x.Role):q.OrderBy(x=>x.Role),_=>q.OrderByDescending(x=>x.Id)};
  var items=q.ToList();return Ok(new AuthUserPagedResult<AuthUserView>(items.Skip((page-1)*pageSize).Take(pageSize).Select(ToView).ToList(),items.Count,page,pageSize));
 }
 [HttpGet("{id:int}")] public IActionResult Get(int id){var x=db.Users.FirstOrDefault(x=>x.Id==id&&!x.IsDeleted);return x is null?NotFound():Ok(ToView(x));}
 [HttpPost] public async Task<IActionResult> Create(CreateAuthUserRequest r,[FromServices]CreateUserHandler handler,CancellationToken ct){var tenant=r.TenantId??User.FindFirst("tenant_id")?.Value??"default";var id=await handler.Handle(new CreateUserCommand(tenant,r.Username??"",r.Email??"",r.FirstName??"",r.LastName??"",r.Password??"",r.Role??"User"),ct);return CreatedAtAction(nameof(Get),new{id},id);}
 [HttpPut("{id:int}")] public async Task<IActionResult> Update(int id,UpdateAuthUserRequest r,[FromServices]UpdateUserHandler handler,CancellationToken ct){var ok=await handler.Handle(new UpdateUserCommand(id,r.Email??"",r.FirstName??"",r.LastName??"",r.Role??"User",r.IsActive,User.IsInRole("SuperUser")),ct);return ok?NoContent():NotFound();}
 [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id,[FromServices]DeleteUserHandler handler,CancellationToken ct)=>await handler.Handle(new DeleteUserCommand(id),ct)?NoContent():NotFound();
 private static AuthUserView ToView(ModernCRM.Auth.Domain.Users.AuthUser x)=>new(x.Id,x.TenantId.Value,x.Username.Value,x.FirstName,x.LastName,x.Email.Value,x.Role.ToString(),x.IsActive,x.IsDeleted,x.IntegrationStatus,x.IntegrationError,DateTime.UtcNow);
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModernCRM.Crm.Api.Frontend;
using ModernCRM.Crm.Api.UserSync;

namespace ModernCRM.Crm.Api.Controllers;

[ApiController, Route("api/accounts")]
public sealed class AccountsController(CrmIntegrationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(int page=1,int pageSize=20,string? sortBy=null,bool sortDesc=false,string? search=null,CancellationToken ct=default)
    { var q=db.Accounts.Where(x=>x.TenantId==FrontendApi.Tenant(User)&&!x.IsDeleted); if(!string.IsNullOrWhiteSpace(search))q=q.Where(x=>(x.Name!=null&&x.Name.Contains(search))||(x.Industry!=null&&x.Industry.Contains(search))); return Ok(await FrontendApi.PageAsync(q,page,pageSize,sortBy,sortDesc,ct)); }
    [HttpGet("{id:int}")] public async Task<IActionResult> Get(int id,CancellationToken ct)=>await Find(id,ct) is { } x?Ok(x):NotFound();
    [HttpPost] public async Task<IActionResult> Create(AccountPayload r,CancellationToken ct){if(string.IsNullOrWhiteSpace(r.Name))return BadRequest(new{message="Account name is required."});var x=new AccountModel{TenantId=FrontendApi.Tenant(User),Name=r.Name,Industry=r.Industry,Website=r.Website,Phone=r.Phone,Address=r.Address,Notes=r.Notes};db.Accounts.Add(x);await db.SaveChangesAsync(ct);return CreatedAtAction(nameof(Get),new{id=x.Id},x.Id);}
    [HttpPut("{id:int}")] public async Task<IActionResult> Update(int id,AccountPayload r,CancellationToken ct){var x=await Find(id,ct);if(x is null)return NotFound();if(string.IsNullOrWhiteSpace(r.Name))return BadRequest(new{message="Account name is required."});x.Name=r.Name;x.Industry=r.Industry;x.Website=r.Website;x.Phone=r.Phone;x.Address=r.Address;x.Notes=r.Notes;await db.SaveChangesAsync(ct);return NoContent();}
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id,CancellationToken ct){var x=await Find(id,ct);if(x is null)return NotFound();x.IsDeleted=true;await db.SaveChangesAsync(ct);return NoContent();}
    private Task<AccountModel?> Find(int id,CancellationToken ct)=>db.Accounts.FirstOrDefaultAsync(x=>x.Id==id&&x.TenantId==FrontendApi.Tenant(User)&&!x.IsDeleted,ct);
}

using Microsoft.AspNetCore.Mvc;
using ModernCRM.Crm.Api.Frontend;

namespace ModernCRM.Crm.Api.Controllers;

[ApiController, Route("api/accounts")]
public sealed class AccountsController(FrontendCrmStore store) : ControllerBase
{
    [HttpGet]
    public ActionResult<PagedResult<AccountModel>> GetAll(int page = 1, int pageSize = 20, string? sortBy = null, bool sortDesc = false, string? search = null)
    {
        lock (store.SyncRoot)
        {
            var tenant = FrontendApi.Tenant(User);
            var query = store.Accounts.Where(x => x.TenantId == tenant && !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => FrontendApi.Contains(x.Name, search) || FrontendApi.Contains(x.Industry, search));
            return Ok(FrontendApi.Page(query, page, pageSize, sortBy, sortDesc));
        }
    }
    [HttpGet("{id:int}")]
    public IActionResult Get(int id) { lock (store.SyncRoot) { var x = store.Accounts.FirstOrDefault(x => x.Id == id && x.TenantId == FrontendApi.Tenant(User) && !x.IsDeleted); return x is null ? NotFound() : Ok(x); } }
    [HttpPost]
    public IActionResult Create(AccountPayload r) { lock (store.SyncRoot) { if (string.IsNullOrWhiteSpace(r.Name)) return BadRequest(new { message = "Account name is required." }); var x = new AccountModel { Id=store.NextId(), TenantId=FrontendApi.Tenant(User), Name=r.Name, Industry=r.Industry, Website=r.Website, Phone=r.Phone, Address=r.Address, Notes=r.Notes }; store.Accounts.Add(x); return CreatedAtAction(nameof(Get), new { id=x.Id }, x.Id); } }
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, AccountPayload r) { lock (store.SyncRoot) { var x=store.Accounts.FirstOrDefault(x=>x.Id==id&&x.TenantId==FrontendApi.Tenant(User)&&!x.IsDeleted); if(x is null)return NotFound(); if(string.IsNullOrWhiteSpace(r.Name))return BadRequest(new{message="Account name is required."}); x.Name=r.Name;x.Industry=r.Industry;x.Website=r.Website;x.Phone=r.Phone;x.Address=r.Address;x.Notes=r.Notes;return NoContent(); } }
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id) { lock(store.SyncRoot){var x=store.Accounts.FirstOrDefault(x=>x.Id==id&&x.TenantId==FrontendApi.Tenant(User)&&!x.IsDeleted);if(x is null)return NotFound();x.IsDeleted=true;return NoContent();} }
}
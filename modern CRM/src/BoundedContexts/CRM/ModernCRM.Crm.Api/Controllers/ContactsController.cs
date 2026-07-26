using Microsoft.AspNetCore.Mvc;
using ModernCRM.Crm.Api.Frontend;

namespace ModernCRM.Crm.Api.Controllers;

[ApiController, Route("api/contacts")]
public sealed class ContactsController(FrontendCrmStore store) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll(int page=1,int pageSize=20,string? sortBy=null,bool sortDesc=false,string? search=null){lock(store.SyncRoot){var tenant=FrontendApi.Tenant(User);var q=store.Contacts.Where(x=>x.TenantId==tenant&&!x.IsDeleted);if(!string.IsNullOrWhiteSpace(search))q=q.Where(x=>FrontendApi.Contains(x.FirstName,search)||FrontendApi.Contains(x.LastName,search)||FrontendApi.Contains(x.Email,search)||FrontendApi.Contains(x.Company,search));return Ok(FrontendApi.Page(q,page,pageSize,sortBy,sortDesc));}}
    [HttpGet("{id:int}")]
    public IActionResult Get(int id){lock(store.SyncRoot){var x=store.Contacts.FirstOrDefault(x=>x.Id==id&&x.TenantId==FrontendApi.Tenant(User)&&!x.IsDeleted);return x is null?NotFound():Ok(x);}}
    [HttpPost]
    public IActionResult Create(Payload r){lock(store.SyncRoot){if(string.IsNullOrWhiteSpace(r.FirstName)||string.IsNullOrWhiteSpace(r.LastName)||string.IsNullOrWhiteSpace(r.Email))return BadRequest(new{message="First name, last name and email are required."});var x=new ContactModel{Id=store.NextId(),TenantId=FrontendApi.Tenant(User),FirstName=r.FirstName,LastName=r.LastName,Email=r.Email,Phone=r.Phone,Company=r.Company,JobTitle=r.JobTitle,Notes=r.Notes,AccountId=r.AccountId};store.Contacts.Add(x);return CreatedAtAction(nameof(Get),new{id=x.Id},x.Id);}}
    [HttpPut("{id:int}")]
    public IActionResult Update(int id,Payload r){lock(store.SyncRoot){var x=store.Contacts.FirstOrDefault(x=>x.Id==id&&x.TenantId==FrontendApi.Tenant(User)&&!x.IsDeleted);if(x is null)return NotFound();x.FirstName=r.FirstName;x.LastName=r.LastName;x.Email=r.Email;x.Phone=r.Phone;x.Company=r.Company;x.JobTitle=r.JobTitle;x.Notes=r.Notes;x.AccountId=r.AccountId;return NoContent();}}
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id){lock(store.SyncRoot){var x=store.Contacts.FirstOrDefault(x=>x.Id==id&&x.TenantId==FrontendApi.Tenant(User)&&!x.IsDeleted);if(x is null)return NotFound();x.IsDeleted=true;return NoContent();}}
    public sealed record Payload(string? FirstName,string? LastName,string? Email,string? Phone,string? Company,string? JobTitle,string? Notes,int? AccountId);
}
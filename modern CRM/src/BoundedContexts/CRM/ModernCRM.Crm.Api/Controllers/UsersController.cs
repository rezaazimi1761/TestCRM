using Microsoft.AspNetCore.Mvc;
using ModernCRM.Crm.Api.Frontend;
using ModernCRM.Crm.Application.Users;

namespace ModernCRM.Crm.Api.Controllers;

[ApiController, Route("api/users")]
public sealed class UsersController(ICrmUserService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(int page = 1, int pageSize = 20, string? sortBy = null, bool sortDesc = false, string? search = null, string? role = null, CancellationToken ct = default)
        => Ok(await service.GetPageAsync(Tenant(), page, pageSize, sortBy, sortDesc, search, role, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
        => await service.GetAsync(Tenant(), id, ct) is { } user ? Ok(user) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create(CreateCrmUserPayload request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Username, email, name and password are required." });
        var result = await service.CreateAsync(Tenant(), new CreateCrmUser(request.Username, request.Email, request.FirstName, request.LastName, request.Password, request.Role ?? "User"), ct);
        return result.Created ? CreatedAtAction(nameof(Get), new { id = result.Id }, result.Id) : Conflict(new { message = "Username or email already exists in CRM." });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateCrmUserPayload request, CancellationToken ct)
        => await service.UpdateAsync(Tenant(), id, new UpdateCrmUser(request.Email, request.FirstName, request.LastName, request.Role, request.IsActive), ct) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await service.DeleteAsync(Tenant(), id, ct) ? NoContent() : NotFound();

    private string Tenant() => FrontendApi.Tenant(User);
}

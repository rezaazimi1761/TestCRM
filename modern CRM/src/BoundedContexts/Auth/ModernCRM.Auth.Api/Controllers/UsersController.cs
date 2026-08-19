using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernCRM.Auth.Application.Commands;
using ModernCRM.Auth.Application.Handlers;
using ModernCRM.Auth.Domain.Users;
using ModernCRM.Auth.Application.Integration;

namespace ModernCRM.Auth.Api.Controllers;

[ApiController, Authorize, Route("api/users")]
public sealed class UsersController(IAuthPersistenceRepository persistence) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(int page = 1, int pageSize = 20, string? sortBy = null, bool sortDesc = false, string? search = null, string? role = null, CancellationToken ct = default)
    {
        var result = await persistence.PageUsersAsync(Tenant(), page, pageSize, sortBy, sortDesc, search, role, ct);
        return Ok(new AuthUserPagedResult<AuthUserView>(result.Items.Select(ToView).ToList(), result.Total, Math.Max(page, 1), Math.Clamp(pageSize < 1 ? 20 : pageSize, 1, 500)));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
        => await persistence.FindUserAsync(Tenant(), id, ct) is { } user ? Ok(ToView(user)) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create(CreateAuthUserRequest request, [FromServices] CreateUserHandler handler, CancellationToken ct)
    {
        var tenant = request.TenantId ?? Tenant();
        if (!User.IsInRole("SuperUser") && !string.Equals(tenant, Tenant(), StringComparison.OrdinalIgnoreCase)) return Forbid();
        var id = await handler.Handle(new CreateUserCommand(tenant, request.Username ?? "", request.Email ?? "", request.FirstName ?? "", request.LastName ?? "", request.Password ?? "", request.Role ?? "User"), ct);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateAuthUserRequest request, [FromServices] UpdateUserHandler handler, CancellationToken ct)
        => await handler.Handle(new UpdateUserCommand(Tenant(), id, request.Email ?? "", request.FirstName ?? "", request.LastName ?? "", request.Role ?? "User", request.IsActive, User.IsInRole("SuperUser")), ct) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, [FromServices] DeleteUserHandler handler, CancellationToken ct)
        => await handler.Handle(new DeleteUserCommand(Tenant(), id), ct) ? NoContent() : NotFound();

    private string Tenant() => User.FindFirst("tenant_id")?.Value
        ?? throw new UnauthorizedAccessException("The authenticated token does not contain a tenant_id claim.");
    private static AuthUserView ToView(AuthUser user) => new(user.Id, user.TenantId.Value, user.Username.Value, user.FirstName, user.LastName, user.Email.Value, user.Role.ToString(), user.IsActive, user.IsDeleted, user.IntegrationStatus, user.IntegrationError, user.CreatedAtUtc);
}

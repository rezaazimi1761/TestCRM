using Microsoft.AspNetCore.Mvc;
using ModernCRM.Auth.Application.Commands;
using ModernCRM.Auth.Application.Handlers;
using ModernCRM.Auth.Application.Queries;

namespace ModernCRM.Auth.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<ModernCRM.Auth.Application.DTO.UserDto>> GetAll([FromQuery] string tenantId, [FromQuery] int page, [FromQuery] int pageSize, [FromQuery] string? search, [FromServices] GetUsersHandler handler, CancellationToken ct)
        => handler.Handle(new GetUsersQuery(tenantId, page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, search), ct);

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, [FromServices] GetUserByIdHandler handler, CancellationToken ct)
        => (await handler.Handle(new GetUserByIdQuery(id), ct)) is { } user ? Ok(user) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command, [FromServices] CreateUserHandler handler, CancellationToken ct)
        => Created($"/api/users/{await handler.Handle(command, ct)}", null);

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserCommand body, [FromServices] UpdateUserHandler handler, CancellationToken ct)
        => await handler.Handle(body with { Id = id }, ct) ? NoContent() : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, [FromServices] DeleteUserHandler handler, CancellationToken ct)
        => await handler.Handle(new DeleteUserCommand(id), ct) ? NoContent() : NotFound();
}

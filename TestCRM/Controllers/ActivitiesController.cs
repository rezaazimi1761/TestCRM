using MediatR;
using Microsoft.AspNetCore.Mvc;
using TestCRM.Application.Common;
using TestCRM.Application.Features.Activities.Commands;
using TestCRM.Application.Features.Activities.Queries;

namespace TestCRM.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivitiesController : ControllerBase
{
    private readonly IMediator _mediator;
    public ActivitiesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null, [FromQuery] bool sortDesc = false,
        [FromQuery] string? search = null,
        [FromQuery] string? type = null,
        [FromQuery] bool? isCompleted = null)
    {
        if (PaginationValidator.ValidateOrBadRequest(page, pageSize) is { } err) return err;
        return Ok(await _mediator.Send(new GetActivitiesQuery(page, pageSize, sortBy, sortDesc, search, type, isCompleted)));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetActivityByIdQuery(id));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateActivityCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateActivityCommand command)
    {
        if (id != command.Id) return BadRequest();
        var updated = await _mediator.Send(command);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _mediator.Send(new DeleteActivityCommand(id));
        return deleted ? NoContent() : NotFound();
    }
}

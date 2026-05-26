using MediatR;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> GetAll() => Ok(await _mediator.Send(new GetActivitiesQuery()));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateActivityCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAll), new { id }, id);
    }
}

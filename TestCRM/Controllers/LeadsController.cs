using MediatR;
using Microsoft.AspNetCore.Mvc;
using TestCRM.Application.Features.Leads.Commands;
using TestCRM.Application.Features.Leads.Queries;

namespace TestCRM.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeadsController : ControllerBase
{
    private readonly IMediator _mediator;
    public LeadsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _mediator.Send(new GetLeadsQuery()));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLeadCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAll), new { id }, id);
    }
}

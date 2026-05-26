using MediatR;
using Microsoft.AspNetCore.Mvc;
using TestCRM.Application.Features.Opportunities.Commands;
using TestCRM.Application.Features.Opportunities.Queries;

namespace TestCRM.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OpportunitiesController : ControllerBase
{
    private readonly IMediator _mediator;
    public OpportunitiesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _mediator.Send(new GetOpportunitiesQuery()));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOpportunityCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAll), new { id }, id);
    }
}

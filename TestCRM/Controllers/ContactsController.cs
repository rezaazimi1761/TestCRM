using MediatR;
using Microsoft.AspNetCore.Mvc;
using TestCRM.Application.Features.Contacts.Commands;
using TestCRM.Application.Features.Contacts.Queries;

namespace TestCRM.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ContactsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _mediator.Send(new GetContactsQuery()));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateContactCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAll), new { id }, id);
    }
}

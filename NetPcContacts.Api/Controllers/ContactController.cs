using MediatR;
using Microsoft.AspNetCore.Mvc;
using NetPcContacts.Application.Contacts.Commands.CreateContact;
using NetPcContacts.Application.Contacts.Commands.DeleteContact;
using NetPcContacts.Application.Contacts.Queries.GetContactById;

namespace NetPcContacts.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ContactController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateContact([FromBody] CreateContactCommand command)
        {
                int id = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetContact), new { id }, new { id });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetContact([FromRoute] int id)
        {
            var contact = await _mediator.Send(new GetContactByIdQuery(id));
            return Ok(contact);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteContact([FromRoute] int id)
        {
            await _mediator.Send(new DeleteContactCommand(id));
            return NoContent();
        }
    }
}

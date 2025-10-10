using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NetPcContacts.Application.Common;
using NetPcContacts.Application.Contacts.Commands.CreateContact;
using NetPcContacts.Application.Contacts.Commands.DeleteContact;
using NetPcContacts.Application.Contacts.Commands.UpdateContact;
using NetPcContacts.Application.Contacts.Dtos;
using NetPcContacts.Application.Contacts.Queries.GetAllContacts;
using NetPcContacts.Application.Contacts.Queries.GetContactById;

namespace NetPcContacts.Api.Controllers
{
    /// <summary>
    /// Kontroler zarządzający operacjami CRUD na kontaktach.
    /// Zgodnie z wymaganiami: przeglądanie listy i szczegółów dostępne publicznie,
    /// tworzenie, edycja i usuwanie wymagają autoryzacji.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ContactController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Tworzy nowy kontakt.
        /// Wymaga autoryzacji - tylko zalogowani użytkownicy mogą dodawać kontakty.
        /// </summary>
        /// <param name="command">Dane nowego kontaktu</param>
        /// <returns>ID utworzonego kontaktu</returns>
        /// <response code="201">Kontakt został utworzony pomyślnie</response>
        /// <response code="400">Nieprawidłowe dane wejściowe (walidacja)</response>
        /// <response code="401">Brak autoryzacji - wymagane zalogowanie</response>
        /// <response code="409">Konflikt - email już istnieje</response>
        /// <response code="429">Zbyt wiele żądań - przekroczony limit</response>
        [HttpPost]
        [Authorize]
        [EnableRateLimiting("commands")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> CreateContact([FromBody] CreateContactCommand command)
        {
            int id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetContact), new { id }, new { id });
        }

        /// <summary>
        /// Aktualizuje istniejący kontakt.
        /// Wymaga autoryzacji - tylko zalogowani użytkownicy mogą edytować kontakty.
        /// </summary>
        /// <param name="id">ID kontaktu do aktualizacji</param>
        /// <param name="command">Zaktualizowane dane kontaktu</param>
        /// <returns>NoContent (204)</returns>
        /// <response code="204">Kontakt został zaktualizowany pomyślnie</response>
        /// <response code="400">Nieprawidłowe dane wejściowe (walidacja)</response>
        /// <response code="401">Brak autoryzacji - wymagane zalogowanie</response>
        /// <response code="404">Kontakt nie został znaleziony</response>
        /// <response code="409">Konflikt - email już istnieje (przy zmianie emaila)</response>
        /// <response code="429">Zbyt wiele żądań - przekroczony limit</response>
        [HttpPatch("{id:int}")]
        [Authorize]
        [EnableRateLimiting("commands")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> UpdateContact([FromRoute] int id, [FromBody] UpdateContactCommand command)
        {
            command.Id = id;
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Pobiera szczegóły pojedynczego kontaktu.
        /// Dostępne publicznie - zgodnie z wymaganiami (pkt. 2 dostępna dla niezalogowanych).
        /// </summary>
        /// <param name="id">ID kontaktu</param>
        /// <returns>Szczegóły kontaktu</returns>
        /// <response code="200">Zwraca szczegóły kontaktu</response>
        /// <response code="404">Kontakt nie został znaleziony</response>
        /// <response code="429">Zbyt wiele żądań - przekroczony limit</response>
        [HttpGet("{id:int}")]
        [EnableRateLimiting("queries")]
        [ProducesResponseType(typeof(ContactDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<ContactDto>> GetContact([FromRoute] int id)
        {
            var contact = await _mediator.Send(new GetContactByIdQuery(id));
            return Ok(contact);
        }

        /// <summary>
        /// Usuwa kontakt.
        /// Wymaga autoryzacji - tylko zalogowani użytkownicy mogą usuwać kontakty.
        /// </summary>
        /// <param name="id">ID kontaktu do usunięcia</param>
        /// <returns>NoContent (204)</returns>
        /// <response code="204">Kontakt został usunięty pomyślnie</response>
        /// <response code="401">Brak autoryzacji - wymagane zalogowanie</response>
        /// <response code="404">Kontakt nie został znaleziony</response>
        /// <response code="429">Zbyt wiele żądań - przekroczony limit</response>
        [HttpDelete("{id:int}")]
        [Authorize]
        [EnableRateLimiting("commands")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> DeleteContact([FromRoute] int id)
        {
            await _mediator.Send(new DeleteContactCommand(id));
            return NoContent();
        }

        /// <summary>
        /// Pobiera wszystkie kontakty z paginacją, wyszukiwaniem i sortowaniem.
        /// Dostępne publicznie - zgodnie z wymaganiami (pkt. 2 dostępna dla niezalogowanych).
        /// </summary>
        /// <param name="query">Parametry zapytania (paginacja, wyszukiwanie, sortowanie)</param>
        /// <returns>Stronicowana lista kontaktów z podstawowymi danymi</returns>
        /// <response code="200">Zwraca stronicowaną listę kontaktów</response>
        /// <response code="400">Nieprawidłowe parametry zapytania (walidacja)</response>
        /// <response code="429">Zbyt wiele żądań - przekroczony limit</response>
        [HttpGet]
        [EnableRateLimiting("queries")]
        [ProducesResponseType(typeof(PagedResult<BasicContactDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<PagedResult<BasicContactDto>>> GetAllContacts([FromQuery] GetAllContactsQuery query)
        {
            var contacts = await _mediator.Send(query);
            return Ok(contacts);
        }
    }
}

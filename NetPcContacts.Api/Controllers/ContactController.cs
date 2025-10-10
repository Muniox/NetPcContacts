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
    /// </summary>
    /// <remarks>
    /// Zgodnie z wymaganiami projektu:
    /// - Przeglądanie listy i szczegółów dostępne publicznie (bez autoryzacji)
    /// - Tworzenie, edycja i usuwanie wymagają autoryzacji (Bearer Token)
    /// - Wszystkie endpointy chronione Rate Limiting
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ContactController : ControllerBase
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// Konstruktor kontrolera kontaktów.
        /// </summary>
        /// <param name="mediator">Mediator CQRS do obsługi komend i zapytań</param>
        public ContactController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Tworzy nowy kontakt w systemie.
        /// </summary>
        /// <param name="command">Dane nowego kontaktu</param>
        /// <returns>ID utworzonego kontaktu</returns>
        /// <remarks>
        /// Przykładowe żądanie:
        /// 
        ///     POST /api/contact
        ///     {
        ///        "name": "Jan",
        ///        "surname": "Kowalski",
        ///        "email": "jan.kowalski@example.com",
        ///        "password": "SecureP@ss123",
        ///        "phoneNumber": "+48 123 456 789",
        ///        "birthDate": "1990-05-15",
        ///        "categoryId": 1,
        ///        "subcategoryId": 2,
        ///        "customSubcategory": null
        ///     }
        /// 
        /// **Wymagania:**
        /// - Wymagana autoryzacja (Bearer Token)
        /// - Email musi być unikalny w systemie
        /// - Hasło musi spełniać standardy złożoności (min. 8 znaków, wielkie/małe litery, cyfry, znaki specjalne)
        /// - CategoryId musi wskazywać na istniejącą kategorię
        /// - SubcategoryId opcjonalny (wymagany dla kategorii "Służbowy")
        /// - CustomSubcategory opcjonalny (używany dla kategorii "Inny")
        /// 
        /// **Rate Limiting:** 30 żądań/minutę
        /// </remarks>
        /// <response code="201">Kontakt został utworzony pomyślnie. Zwraca ID utworzonego kontaktu.</response>
        /// <response code="400">Nieprawidłowe dane wejściowe. Sprawdź walidację pól.</response>
        /// <response code="401">Brak autoryzacji. Wymagane zalogowanie i podanie Bearer Token.</response>
        /// <response code="409">Konflikt - kontakt o podanym adresie email już istnieje.</response>
        /// <response code="429">Zbyt wiele żądań. Przekroczono limit 30 żądań/minutę.</response>
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
        /// </summary>
        /// <param name="id">ID kontaktu do aktualizacji</param>
        /// <param name="command">Zaktualizowane dane kontaktu</param>
        /// <returns>NoContent (204)</returns>
        /// <remarks>
        /// Przykładowe żądanie:
        /// 
        ///     PATCH /api/contact/5
        ///     {
        ///        "name": "Jan",
        ///        "surname": "Nowak",
        ///        "email": "jan.nowak@example.com",
        ///        "password": null,
        ///        "phoneNumber": "+48 987 654 321",
        ///        "birthDate": "1990-05-15",
        ///        "categoryId": 2,
        ///        "subcategoryId": null,
        ///        "customSubcategory": "Partner biznesowy"
        ///     }
        /// 
        /// **Wymagania:**
        /// - Wymagana autoryzacja (Bearer Token)
        /// - ID musi wskazywać na istniejący kontakt
        /// - Email musi być unikalny (jeśli zmieniany)
        /// - Hasło opcjonalne - jeśli null, nie jest zmieniane
        /// - Pozostałe pola jak przy tworzeniu
        /// 
        /// **Rate Limiting:** 30 żądań/minutę
        /// </remarks>
        /// <response code="204">Kontakt został zaktualizowany pomyślnie.</response>
        /// <response code="400">Nieprawidłowe dane wejściowe.</response>
        /// <response code="401">Brak autoryzacji.</response>
        /// <response code="404">Kontakt o podanym ID nie został znaleziony.</response>
        /// <response code="409">Konflikt - email już istnieje (przy zmianie emaila).</response>
        /// <response code="429">Zbyt wiele żądań.</response>
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
        /// </summary>
        /// <param name="id">ID kontaktu</param>
        /// <returns>Szczegóły kontaktu</returns>
        /// <remarks>
        /// Endpoint publiczny - nie wymaga autoryzacji zgodnie z wymaganiami projektu.
        /// 
        /// Przykładowe żądanie:
        /// 
        ///     GET /api/contact/5
        /// 
        /// Zwraca pełne informacje o kontakcie, w tym:
        /// - Dane osobowe (imię, nazwisko, email, telefon, data urodzenia)
        /// - Kategoria i podkategoria
        /// - **UWAGA:** Hasło NIE jest zwracane (bezpieczeństwo)
        /// 
        /// **Rate Limiting:** 100 tokenów/minutę
        /// </remarks>
        /// <response code="200">Zwraca szczegóły kontaktu.</response>
        /// <response code="404">Kontakt o podanym ID nie został znaleziony.</response>
        /// <response code="429">Zbyt wiele żądań.</response>
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
        /// Usuwa kontakt z systemu.
        /// </summary>
        /// <param name="id">ID kontaktu do usunięcia</param>
        /// <returns>NoContent (204)</returns>
        /// <remarks>
        /// Przykładowe żądanie:
        /// 
        ///     DELETE /api/contact/5
        /// 
        /// **Wymagania:**
        /// - Wymagana autoryzacja (Bearer Token)
        /// - Operacja nieodwracalna - kontakt zostanie trwale usunięty
        /// 
        /// **Rate Limiting:** 30 żądań/minutę
        /// </remarks>
        /// <response code="204">Kontakt został usunięty pomyślnie.</response>
        /// <response code="401">Brak autoryzacji.</response>
        /// <response code="404">Kontakt o podanym ID nie został znaleziony.</response>
        /// <response code="429">Zbyt wiele żądań.</response>
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
        /// Pobiera listę wszystkich kontaktów z paginacją, wyszukiwaniem i sortowaniem.
        /// </summary>
        /// <param name="query">Parametry zapytania (paginacja, wyszukiwanie, sortowanie)</param>
        /// <returns>Stronicowana lista kontaktów z podstawowymi danymi</returns>
        /// <remarks>
        /// Endpoint publiczny - nie wymaga autoryzacji zgodnie z wymaganiami projektu.
        /// 
        /// Przykładowe żądanie:
        /// 
        ///     GET /api/contact?SearchPhrase=jan&amp;PageNumber=1&amp;PageSize=10&amp;SortBy=FirstName&amp;SortDirection=Ascending
        /// 
        /// **Parametry:**
        /// - **SearchPhrase** (opcjonalny) - wyszukuje po imieniu, nazwisku lub emailu
        /// - **PageNumber** (wymagany) - numer strony (min. 1)
        /// - **PageSize** (wymagany) - rozmiar strony: 5, 10, 15 lub 30
        /// - **SortBy** (opcjonalny) - kolumna do sortowania: FirstName, LastName, Category
        /// - **SortDirection** (wymagany) - kierunek: Ascending (0) lub Descending (1)
        /// 
        /// **Zwracane dane:**
        /// - Lista podstawowych informacji o kontaktach (imię, nazwisko, email, telefon, kategoria)
        /// - Metadane paginacji (totalPages, totalItemsCount, itemsFrom, itemsTo)
        /// 
        /// **Rate Limiting:** 100 tokenów/minutę
        /// </remarks>
        /// <response code="200">Zwraca stronicowaną listę kontaktów.</response>
        /// <response code="400">Nieprawidłowe parametry zapytania (np. niewłaściwy PageSize).</response>
        /// <response code="429">Zbyt wiele żądań.</response>
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

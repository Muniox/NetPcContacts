using MediatR;
using Microsoft.Extensions.Logging;
using NetPcContacts.Application.Common;
using NetPcContacts.Application.Contacts.Dtos;
using NetPcContacts.Domain.IRepositories;

namespace NetPcContacts.Application.Contacts.Queries.GetAllContacts
{
    /// <summary>
    /// Handler obsługujący zapytanie GetAllContactsQuery.
    /// Pobiera stronicowaną listę kontaktów z możliwością wyszukiwania i sortowania.
    /// </summary>
    public class GetAllContactsQueryHandler : IRequestHandler<GetAllContactsQuery, PagedResult<BasicContactDto>>
    {
        private readonly IContactsRepository _contactsRepository;
        private readonly ILogger<GetAllContactsQueryHandler> _logger;

        /// <summary>
        /// Konstruktor wstrzykujący zależności.
        /// </summary>
        /// <param name="contactsRepository">Repozytorium kontaktów</param>
        /// <param name="logger">Logger do logowania operacji</param>
        public GetAllContactsQueryHandler(
            IContactsRepository contactsRepository,
            ILogger<GetAllContactsQueryHandler> logger)
        {
            _contactsRepository = contactsRepository;
            _logger = logger;
        }

        /// <summary>
        /// Obsługuje zapytanie pobierające stronicowaną listę kontaktów.
        /// </summary>
        /// <param name="request">Query zawierające parametry wyszukiwania, paginacji i sortowania</param>
        /// <param name="cancellationToken">Token anulowania operacji</param>
        /// <returns>PagedResult z listą BasicContactDto oraz metadanymi paginacji</returns>
        public async Task<PagedResult<BasicContactDto>> Handle(GetAllContactsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Retrieving contacts with pagination - Page: {PageNumber}, PageSize: {PageSize}, SearchPhrase: {SearchPhrase}, SortBy: {SortBy}, SortDirection: {SortDirection}",
                request.PageNumber,
                request.PageSize,
                request.SearchPhrase ?? "null",
                request.SortBy ?? "null",
                request.SortDirection);

            // Pobierz kontakty z bazy z uwzględnieniem filtrowania, sortowania i paginacji
            var (contacts, totalCount) = await _contactsRepository.GetAllMatchingAsync(
                request.SearchPhrase,
                request.PageSize,
                request.PageNumber,
                request.SortBy,
                request.SortDirection);

            // Mapuj encje Contact na BasicContactDto
            var contactsDtos = contacts.Select(c => new BasicContactDto
            {
                Id = c.Id,
                FirstName = c.Name,
                LastName = c.Surname,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                Category = c.Category?.CategoryName ?? "Unknown"
            });

            _logger.LogInformation(
                "Retrieved {Count} contacts out of {TotalCount} total contacts",
                contactsDtos.Count(),
                totalCount);

            // Zwróć stronicowany wynik
            return new PagedResult<BasicContactDto>(
                contactsDtos,
                totalCount,
                request.PageSize,
                request.PageNumber);
        }
    }
}

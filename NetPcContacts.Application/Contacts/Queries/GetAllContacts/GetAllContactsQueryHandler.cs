using Mediator;
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
    public class GetAllContactsQueryHandler : IQueryHandler<GetAllContactsQuery, PagedResult<BasicContactDto>>
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
        /// <param name="query">Query zawierające parametry wyszukiwania, paginacji i sortowania</param>
        /// <param name="cancellationToken">Token anulowania operacji</param>
        /// <returns>PagedResult z listą BasicContactDto oraz metadanymi paginacji</returns>
        public async ValueTask<PagedResult<BasicContactDto>> Handle(GetAllContactsQuery query, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Retrieving contacts with pagination - Page: {PageNumber}, PageSize: {PageSize}, SearchPhrase: {SearchPhrase}, SortBy: {SortBy}, SortDirection: {SortDirection}",
                query.PageNumber,
                query.PageSize,
                query.SearchPhrase ?? "null",
                query.SortBy ?? "null",
                query.SortDirection);

            // Pobierz kontakty z bazy z uwzględnieniem filtrowania, sortowania i paginacji
            var (contacts, totalCount) = await _contactsRepository.GetAllMatchingAsync(
                query.SearchPhrase,
                query.PageSize,
                query.PageNumber,
                query.SortBy,
                query.SortDirection);

            // Mapuj encje Contact na BasicContactDto
            var contactsDtos = contacts.Select(c => new BasicContactDto
            {
                Id = c.Id,
                Name = c.Name,
                Surname = c.Surname,
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
                query.PageSize,
                query.PageNumber);
        }
    }
}

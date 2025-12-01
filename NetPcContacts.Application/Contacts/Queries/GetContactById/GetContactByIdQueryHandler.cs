using Mediator;
using Microsoft.Extensions.Logging;
using NetPcContacts.Application.Contacts.Dtos;
using NetPcContacts.Domain.Exceptions;
using NetPcContacts.Domain.IRepositories;

namespace NetPcContacts.Application.Contacts.Queries.GetContactById
{
    /// <summary>
    /// Handler obsługujący zapytanie GetContactByIdQuery.
    /// Pobiera kontakt z bazy i mapuje go na ContactDto.
    /// </summary>
    public class GetContactByIdQueryHandler : IQueryHandler<GetContactByIdQuery, ContactDto?>
    {
        private readonly IContactsRepository _contactsRepository;
        private readonly ILogger<GetContactByIdQueryHandler> _logger;

        public GetContactByIdQueryHandler(IContactsRepository contactsRepository, ILogger<GetContactByIdQueryHandler> logger)
        {
            _contactsRepository = contactsRepository;
            _logger = logger;

        }

        /// <summary>
        /// Obsługuje zapytanie pobierające kontakt po ID.
        /// </summary>
        /// <param name="query">Query zawierające ID kontaktu</param>
        /// <param name="cancellationToken">Token anulowania operacji</param>
        /// <returns>ContactDto z danymi kontaktu lub null jeśli nie znaleziono</returns>
        public async ValueTask<ContactDto?> Handle(GetContactByIdQuery query, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving contact: {CotactId}", query.ContactId);

            // Pobierz kontakt z bazy wraz z relacjami (Category, Subcategory)
            var contact = await _contactsRepository.GetById(query.ContactId);

            // Jeśli kontakt nie istnieje, zwróć null
            if (contact == null)
            {
                throw new NotFoundException(nameof(contact), query.ContactId.ToString());
            }

            _logger.LogInformation("Contact retrieved successfully: {ContactId}", query.ContactId);

            // Mapuj encję Contact na ContactDto
            // WAŻNE: NIE zwracamy PasswordHash - bezpieczeństwo!
            return new ContactDto
            {
                Id = contact.Id,
                Name = contact.Name,
                Surname = contact.Surname,
                Email = contact.Email,
                PhoneNumber = contact.PhoneNumber,
                BirthDate = contact.BirthDate,
                CategoryId = contact.CategoryId,
                CategoryName = contact.Category?.CategoryName ?? "Unknown",
                SubcategoryId = contact.SubcategoryId,
                SubcategoryName = contact.Subcategory?.SubcategoryName,
                CustomSubcategory = contact.CustomSubcategory
            };
        }
    }
}

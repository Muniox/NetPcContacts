using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NetPcContacts.Domain.Entities;
using NetPcContacts.Domain.Exceptions;
using NetPcContacts.Domain.IRepositories;

namespace NetPcContacts.Application.Contacts.Commands.UpdateContact
{
    /// <summary>
    /// Handler obsługujący komendę aktualizacji kontaktu.
    /// Implementuje wzorzec CQRS poprzez MediatR.
    /// Wykonuje walidację biznesową oraz aktualizację danych w bazie.
    /// </summary>
    public class UpdateContactCommandHandler : IRequestHandler<UpdateContactCommand>
    {
        private readonly IContactsRepository _contactsRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISubcategoryRepository _subcategoryRepository;
        private readonly IPasswordHasher<Contact> _passwordHasher;
        private readonly ILogger<UpdateContactCommandHandler> _logger;

        /// <summary>
        /// Konstruktor wstrzykujący zależności.
        /// </summary>
        /// <param name="contactsRepository">Repozytorium kontaktów</param>
        /// <param name="categoryRepository">Repozytorium kategorii</param>
        /// <param name="subcategoryRepository">Repozytorium podkategorii</param>
        /// <param name="passwordHasher">Hasher haseł z ASP.NET Core Identity</param>
        /// <param name="logger">Logger do rejestrowania operacji</param>
        public UpdateContactCommandHandler(
            IContactsRepository contactsRepository,
            ICategoryRepository categoryRepository,
            ISubcategoryRepository subcategoryRepository,
            IPasswordHasher<Contact> passwordHasher,
            ILogger<UpdateContactCommandHandler> logger)
        {
            _contactsRepository = contactsRepository;
            _categoryRepository = categoryRepository;
            _subcategoryRepository = subcategoryRepository;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        /// <summary>
        /// Obsługuje komendę UpdateContactCommand.
        /// Przeprowadza pełną walidację biznesową i aktualizuje kontakt w bazie.
        /// </summary>
        /// <param name="request">Komenda zawierająca dane do aktualizacji kontaktu</param>
        /// <param name="cancellationToken">Token anulowania operacji</param>
        /// <exception cref="NotFoundException">Rzucany gdy kontakt, kategoria lub podkategoria nie istnieją</exception>
        /// <exception cref="DuplicateEmailException">Rzucany gdy email jest już używany przez inny kontakt</exception>
        public async Task Handle(UpdateContactCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating contact: {ContactId}", request.Id);

            // KROK 1: Sprawdź czy kontakt istnieje
            var contact = await _contactsRepository.GetById(request.Id);
            if (contact == null)
            {
                _logger.LogWarning("Contact not found: {ContactId}", request.Id);
                throw new NotFoundException(nameof(contact), request.Id.ToString());
            }

            // KROK 2: Walidacja unikalności emaila (jeśli email się zmienił)
            // Email musi być unikalny w całym systemie, ale dozwolone jest pozostawienie obecnego emaila
            if (contact.Email != request.Email)
            {
                var emailExists = await _contactsRepository.EmailExists(request.Email);
                if (emailExists)
                {
                    _logger.LogWarning("Email already exists: {Email}", request.Email);
                    throw new DuplicateEmailException(request.Email);
                }
            }

            // KROK 3: Walidacja istnienia kategorii
            // CategoryId musi wskazywać na istniejący rekord w tabeli Categories
            var categoryExists = await _categoryRepository.Exists(request.CategoryId);
            if (!categoryExists)
            {
                _logger.LogWarning("Category not found: {CategoryId}", request.CategoryId);
                throw new NotFoundException(nameof(request.CategoryId), request.CategoryId.ToString());
            }

            // KROK 4: Walidacja podkategorii (jeśli podana)
            // SubcategoryId musi:
            // - istnieć w tabeli Subcategories
            // - należeć do kategorii wskazanej przez CategoryId
            if (request.SubcategoryId.HasValue)
            {
                var subcategoryValid = await _subcategoryRepository.ExistsForCategory(
                    request.SubcategoryId.Value,
                    request.CategoryId);

                if (!subcategoryValid)
                {
                    _logger.LogWarning("Subcategory not found or doesn't belong to category: SubcategoryId={SubcategoryId}, CategoryId={CategoryId}",
                        request.SubcategoryId.Value, request.CategoryId);
                    throw new NotFoundException(nameof(request.SubcategoryId), request.SubcategoryId.Value.ToString());
                }
            }

            // KROK 5: Aktualizacja danych kontaktu
            contact.Name = request.Name;
            contact.Surname = request.Surname;
            contact.Email = request.Email;
            contact.PhoneNumber = request.PhoneNumber;
            contact.BirthDate = request.BirthDate;
            contact.CategoryId = request.CategoryId;
            contact.SubcategoryId = request.SubcategoryId;
            contact.CustomSubcategory = request.CustomSubcategory;

            // KROK 6: Aktualizacja hasła (jeśli podane)
            // NIGDY nie przechowujemy haseł w postaci jawnej!
            // Hashujemy hasło tylko jeśli zostało podane nowe hasło
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                contact.PasswordHash = _passwordHasher.HashPassword(contact, request.Password);
                _logger.LogInformation("Password updated for contact: {ContactId}", request.Id);
            }

            // KROK 7: Zapis zmian do bazy danych
            await _contactsRepository.SaveChanges();

            _logger.LogInformation("Contact updated successfully: {ContactId}", request.Id);
        }
    }
}

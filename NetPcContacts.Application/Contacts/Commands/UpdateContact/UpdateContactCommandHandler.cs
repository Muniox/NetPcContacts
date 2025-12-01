using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NetPcContacts.Domain.Entities;
using NetPcContacts.Domain.Exceptions;
using NetPcContacts.Domain.IRepositories;

namespace NetPcContacts.Application.Contacts.Commands.UpdateContact
{
    /// <summary>
    /// Handler obsługujący komendę aktualizacji kontaktu.
    /// Implementuje wzorzec CQRS poprzez Mediator.
    /// Wykonuje walidację biznesową oraz aktualizację danych w bazie.
    /// </summary>
    public class UpdateContactCommandHandler : ICommandHandler<UpdateContactCommand>
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
        /// <param name="command">Komenda zawierająca dane do aktualizacji kontaktu</param>
        /// <param name="cancellationToken">Token anulowania operacji</param>
        /// <exception cref="NotFoundException">Rzucany gdy kontakt, kategoria lub podkategoria nie istnieją</exception>
        /// <exception cref="DuplicateEmailException">Rzucany gdy email jest już używany przez inny kontakt</exception>
        public async ValueTask<Unit> Handle(UpdateContactCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating contact: {ContactId}", command.Id);

            // KROK 1: Sprawdź czy kontakt istnieje
            var contact = await _contactsRepository.GetById(command.Id);
            if (contact == null)
            {
                _logger.LogWarning("Contact not found: {ContactId}", command.Id);
                throw new NotFoundException(nameof(contact), command.Id.ToString());
            }

            // KROK 2: Walidacja unikalności emaila (jeśli email się zmienił)
            // Email musi być unikalny w całym systemie, ale dozwolone jest pozostawienie obecnego emaila
            if (contact.Email != command.Email)
            {
                var emailExists = await _contactsRepository.EmailExists(command.Email);
                if (emailExists)
                {
                    _logger.LogWarning("Email already exists: {Email}", command.Email);
                    throw new DuplicateEmailException(command.Email);
                }
            }

            // KROK 3: Walidacja istnienia kategorii
            // CategoryId musi wskazywać na istniejący rekord w tabeli Categories
            var categoryExists = await _categoryRepository.Exists(command.CategoryId);
            if (!categoryExists)
            {
                _logger.LogWarning("Category not found: {CategoryId}", command.CategoryId);
                throw new NotFoundException(nameof(command.CategoryId), command.CategoryId.ToString());
            }

            // KROK 4: Walidacja podkategorii (jeśli podana)
            // SubcategoryId musi:
            // - istnieć w tabeli Subcategories
            // - należeć do kategorii wskazanej przez CategoryId
            if (command.SubcategoryId.HasValue)
            {
                var subcategoryValid = await _subcategoryRepository.ExistsForCategory(
                    command.SubcategoryId.Value,
                    command.CategoryId);

                if (!subcategoryValid)
                {
                    _logger.LogWarning("Subcategory not found or doesn't belong to category: SubcategoryId={SubcategoryId}, CategoryId={CategoryId}",
                        command.SubcategoryId.Value, command.CategoryId);
                    throw new NotFoundException(nameof(command.SubcategoryId), command.SubcategoryId.Value.ToString());
                }
            }

            // KROK 5: Aktualizacja danych kontaktu
            contact.Name = command.Name;
            contact.Surname = command.Surname;
            contact.Email = command.Email;
            contact.PhoneNumber = command.PhoneNumber;
            contact.BirthDate = command.BirthDate;
            contact.CategoryId = command.CategoryId;
            contact.SubcategoryId = command.SubcategoryId;
            contact.CustomSubcategory = command.CustomSubcategory;

            // KROK 6: Aktualizacja hasła (jeśli podane)
            // NIGDY nie przechowujemy haseł w postaci jawnej!
            // Hashujemy hasło tylko jeśli zostało podane nowe hasło
            if (!string.IsNullOrWhiteSpace(command.Password))
            {
                contact.PasswordHash = _passwordHasher.HashPassword(contact, command.Password);
                _logger.LogInformation("Password updated for contact: {ContactId}", command.Id);
            }

            // KROK 7: Zapis zmian do bazy danych
            await _contactsRepository.SaveChanges();

            _logger.LogInformation("Contact updated successfully: {ContactId}", command.Id);
            
            return Unit.Value;
        }
    }
}

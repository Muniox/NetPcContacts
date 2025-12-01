using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NetPcContacts.Domain.Entities;
using NetPcContacts.Domain.Exceptions;
using NetPcContacts.Domain.IRepositories;

namespace NetPcContacts.Application.Contacts.Commands.CreateContact
{
    /// <summary>
    /// Handler obsługujący komendę tworzenia nowego kontaktu.
    /// Implementuje wzorzec CQRS poprzez Mediator.
    /// Wykonuje walidację biznesową, hashowanie hasła oraz zapis do bazy danych.
    /// </summary>
    public class CreateContactCommandHandler : ICommandHandler<CreateContactCommand, int>
    {
        private readonly ILogger<CreateContactCommandHandler> _logger;
        private readonly IContactsRepository _contactsRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISubcategoryRepository _subcategoryRepository;
        private readonly IPasswordHasher<Contact> _passwordHasher;

        /// <summary>
        /// Konstruktor wstrzykujący zależności.
        /// </summary>
        /// <param name="contactsRepository">Repozytorium kontaktów</param>
        /// <param name="categoryRepository">Repozytorium kategorii</param>
        /// <param name="subcategoryRepository">Repozytorium podkategorii</param>
        /// <param name="passwordHasher">Hasher haseł z ASP.NET Core Identity</param>
        public CreateContactCommandHandler(
            ILogger<CreateContactCommandHandler> logger,
            IContactsRepository contactsRepository,
            ICategoryRepository categoryRepository,
            ISubcategoryRepository subcategoryRepository,
            IPasswordHasher<Contact> passwordHasher)
        {
            _logger = logger;
            _contactsRepository = contactsRepository;
            _categoryRepository = categoryRepository;
            _subcategoryRepository = subcategoryRepository;
            _passwordHasher = passwordHasher;
        }

        /// <summary>
        /// Obsługuje komendę CreateContactCommand.
        /// Przeprowadza pełną walidację biznesową i tworzy nowy kontakt w bazie.
        /// </summary>
        /// <param name="command">Komenda zawierająca dane nowego kontaktu</param>
        /// <param name="cancellationToken">Token anulowania operacji</param>
        /// <returns>ID utworzonego kontaktu</returns>
        /// <exception cref="InvalidOperationException">
        /// Rzucany gdy:
        /// - Email już istnieje w systemie
        /// - CategoryId nie istnieje
        /// - SubcategoryId nie istnieje lub nie należy do CategoryId
        /// </exception>
        public async ValueTask<int> Handle(CreateContactCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating new contact with email: {Email}", command.Email);
            // KROK 1: Walidacja unikalności emaila
            // Email musi być unikalny w całym systemie (wymaganie z task.md)
            var emailExists = await _contactsRepository.EmailExists(command.Email);
            if (emailExists)
            {
                throw new DuplicateEmailException(command.Email);
            }

            // KROK 2: Walidacja istnienia kategorii
            // CategoryId musi wskazywać na istniejący rekord w tabeli Categories
            var categoryExists = await _categoryRepository.Exists(command.CategoryId);
            if (!categoryExists)
            {
                throw new NotFoundException(nameof(command.CategoryId), command.CategoryId.ToString());
            }

            // KROK 3: Walidacja podkategorii (jeśli podana)
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
                    throw new NotFoundException(nameof(command.SubcategoryId), command.SubcategoryId.Value.ToString());
                }
            }

            // KROK 4: Hashowanie hasła
            // NIGDY nie przechowujemy haseł w postaci jawnej!
            // Używamy IPasswordHasher<Contact> z ASP.NET Core Identity
            var contact = new Contact
            {
                Name = command.Name,
                Surname = command.Surname,
                Email = command.Email,
                PhoneNumber = command.PhoneNumber,
                BirthDate = command.BirthDate,
                CategoryId = command.CategoryId,
                SubcategoryId = command.SubcategoryId,
                CustomSubcategory = command.CustomSubcategory
            };

            // Hash hasła używając wbudowanego algorytmu Identity (PBKDF2 z solą)
            contact.PasswordHash = _passwordHasher.HashPassword(contact, command.Password);

            // KROK 5: Zapis do bazy danych
            // Repository wywołuje SaveChangesAsync wewnątrz metody Create
            var contactId = await _contactsRepository.Create(contact);

            _logger.LogInformation("Contact created successfully with ID: {ContactId}", contactId);

            // KROK 6: Zwrócenie ID utworzonego kontaktu
            return contactId;
        }
    }
}

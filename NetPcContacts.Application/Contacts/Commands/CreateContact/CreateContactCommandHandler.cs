using MediatR;
using Microsoft.AspNetCore.Identity;
using NetPcContacts.Domain.Entities;
using NetPcContacts.Domain.IRepositories;

namespace NetPcContacts.Application.Contacts.Commands.CreateContact
{
    /// <summary>
    /// Handler obsługujący komendę tworzenia nowego kontaktu.
    /// Implementuje wzorzec CQRS poprzez MediatR.
    /// Wykonuje walidację biznesową, hashowanie hasła oraz zapis do bazy danych.
    /// </summary>
    public class CreateContactCommandHandler : IRequestHandler<CreateContactCommand, int>
    {
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
            IContactsRepository contactsRepository,
            ICategoryRepository categoryRepository,
            ISubcategoryRepository subcategoryRepository,
            IPasswordHasher<Contact> passwordHasher)
        {
            _contactsRepository = contactsRepository;
            _categoryRepository = categoryRepository;
            _subcategoryRepository = subcategoryRepository;
            _passwordHasher = passwordHasher;
        }

        /// <summary>
        /// Obsługuje komendę CreateContactCommand.
        /// Przeprowadza pełną walidację biznesową i tworzy nowy kontakt w bazie.
        /// </summary>
        /// <param name="request">Komenda zawierająca dane nowego kontaktu</param>
        /// <param name="cancellationToken">Token anulowania operacji</param>
        /// <returns>ID utworzonego kontaktu</returns>
        /// <exception cref="InvalidOperationException">
        /// Rzucany gdy:
        /// - Email już istnieje w systemie
        /// - CategoryId nie istnieje
        /// - SubcategoryId nie istnieje lub nie należy do CategoryId
        /// </exception>
        public async Task<int> Handle(CreateContactCommand request, CancellationToken cancellationToken)
        {
            // KROK 1: Walidacja unikalności emaila
            // Email musi być unikalny w całym systemie (wymaganie z task.md)
            var emailExists = await _contactsRepository.EmailExists(request.Email);
            if (emailExists)
            {
                throw new InvalidOperationException($"Kontakt z emailem '{request.Email}' już istnieje w systemie.");
            }

            // KROK 2: Walidacja istnienia kategorii
            // CategoryId musi wskazywać na istniejący rekord w tabeli Categories
            var categoryExists = await _categoryRepository.Exists(request.CategoryId);
            if (!categoryExists)
            {
                throw new InvalidOperationException($"Kategoria o ID '{request.CategoryId}' nie istnieje.");
            }

            // KROK 3: Walidacja podkategorii (jeśli podana)
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
                    throw new InvalidOperationException(
                        $"Podkategoria o ID '{request.SubcategoryId.Value}' nie istnieje " +
                        $"lub nie należy do kategorii o ID '{request.CategoryId}'.")
                        {
                            Data = { ["SubcategoryId"] = request.SubcategoryId.Value, ["CategoryId"] = request.CategoryId }
                        };
                }
            }

            // KROK 4: Hashowanie hasła
            // NIGDY nie przechowujemy haseł w postaci jawnej!
            // Używamy IPasswordHasher<Contact> z ASP.NET Core Identity
            var contact = new Contact
            {
                Name = request.Name,
                Surname = request.Surname,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                BirthDate = request.BirthDate,
                CategoryId = request.CategoryId,
                SubcategoryId = request.SubcategoryId,
                CustomSubcategory = request.CustomSubcategory
            };

            // Hash hasła używając wbudowanego algorytmu Identity (PBKDF2 z solą)
            contact.PasswordHash = _passwordHasher.HashPassword(contact, request.Password);

            // KROK 5: Zapis do bazy danych
            // Repository wywołuje SaveChangesAsync wewnątrz metody Create
            var contactId = await _contactsRepository.Create(contact);

            // KROK 6: Zwrócenie ID utworzonego kontaktu
            return contactId;
        }
    }
}

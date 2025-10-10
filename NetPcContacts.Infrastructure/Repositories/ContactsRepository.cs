using Microsoft.EntityFrameworkCore;
using NetPcContacts.Domain.Constants;
using NetPcContacts.Domain.Entities;
using NetPcContacts.Domain.IRepositories;
using NetPcContacts.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace NetPcContacts.Infrastructure.Repositories
{
    /// <summary>
    /// Implementacja repozytorium dla encji Contact.
    /// Zapewnia dostęp do danych kontaktów w bazie przy użyciu Entity Framework Core.
    /// </summary>
    internal class ContactsRepository : IContactsRepository
    {
        private readonly NetPcContactsDbContext _dbContext;

        public ContactsRepository(NetPcContactsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Tworzy nowy kontakt w bazie danych.
        /// SaveChangesAsync generuje ID, które jest zwracane.
        /// </summary>
        public async Task<int> Create(Contact contact)
        {
            _dbContext.Contacts.Add(contact);
            await _dbContext.SaveChangesAsync();
            return contact.Id;
        }

        /// <summary>
        /// Usuwa kontakt z bazy danych.
        /// </summary>
        public async Task Delete(Contact contact)
        {
            _dbContext.Contacts.Remove(contact);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Pobiera wszystkie kontakty wraz z ich relacjami.
        /// Include ładuje powiązane encje (eager loading).
        /// </summary>
        public async Task<IEnumerable<Contact>> GetAll()
        {
            return await _dbContext.Contacts
                .Include(c => c.Category)
                .Include(c => c.Subcategory)
                .ToListAsync();
        }

        /// <summary>
        /// Pobiera kontakt po ID wraz z relacjami.
        /// </summary>
        public async Task<Contact?> GetById(int id)
        {
            return await _dbContext.Contacts
                .Include(c => c.Category)
                .Include(c => c.Subcategory)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <summary>
        /// Pobiera kontakt po adresie email.
        /// Używane podczas logowania oraz walidacji unikalności emaila.
        /// </summary>
        public async Task<Contact?> GetByEmail(string email)
        {
            return await _dbContext.Contacts
                .Include(c => c.Category)
                .Include(c => c.Subcategory)
                .FirstOrDefaultAsync(c => c.Email == email);
        }

        /// <summary>
        /// Sprawdza czy email już istnieje w bazie.
        /// Używa AnyAsync dla wydajności (nie pobiera całej encji, tylko sprawdza istnienie).
        /// </summary>
        public async Task<bool> EmailExists(string email)
        {
            return await _dbContext.Contacts
                .AnyAsync(c => c.Email == email);
        }

        /// <summary>
        /// Zapisuje zmiany w encjach śledzonych przez kontekst.
        /// Używane po modyfikacji istniejącego kontaktu (Update).
        /// </summary>
        public async Task SaveChanges()
        {
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Pobiera stronicowaną listę kontaktów spełniających kryteria wyszukiwania z możliwością sortowania.
        /// </summary>
        public async Task<(IEnumerable<Contact>, int)> GetAllMatchingAsync(string? searchPhrase, 
            int pageSize, 
            int pageNumber,
            string? sortBy,
            SortDirection sortDirection)
        {
            var searchPhraseLower = searchPhrase?.ToLower();

            var baseQuery = _dbContext
                .Contacts
                .Include(c => c.Category)
                .Include(c => c.Subcategory)
                .Where(c => searchPhraseLower == null || (c.Name.ToLower().Contains(searchPhraseLower)
                                                       || c.Surname.ToLower().Contains(searchPhraseLower)
                                                       || c.Email.ToLower().Contains(searchPhraseLower)));

            var totalCount = await baseQuery.CountAsync();

            if(sortBy != null)
            {
                var columnsSelector = new Dictionary<string, Expression<Func<Contact, object>>>
                {
                    { "FirstName", c => c.Name },
                    { "LastName", c => c.Surname },
                    { "Category", c => c.Category.CategoryName },
                };

                var selectedColumn = columnsSelector[sortBy];

                baseQuery = sortDirection == SortDirection.Ascending
                    ? baseQuery.OrderBy(selectedColumn)
                    : baseQuery.OrderByDescending(selectedColumn);
            }

            var contacts = await baseQuery
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToListAsync();

            return (contacts, totalCount);
        }
    }
}

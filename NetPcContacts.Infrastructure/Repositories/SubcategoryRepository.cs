using Microsoft.EntityFrameworkCore;
using NetPcContacts.Domain.Entities;
using NetPcContacts.Domain.IRepositories;
using NetPcContacts.Infrastructure.Persistence;

namespace NetPcContacts.Infrastructure.Repositories
{
    /// <summary>
    /// Implementacja repozytorium dla encji Subcategory.
    /// Zapewnia dostęp do danych podkategorii w bazie przy użyciu Entity Framework Core.
    /// </summary>
    internal class SubcategoryRepository : ISubcategoryRepository
    {
        private readonly NetPcContactsDbContext _dbContext;

        public SubcategoryRepository(NetPcContactsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Tworzy nową podkategorię w bazie danych.
        /// </summary>
        public async Task<int> Create(Subcategory subcategory)
        {
            _dbContext.Subcategories.Add(subcategory);
            await _dbContext.SaveChangesAsync();
            return subcategory.Id;
        }

        /// <summary>
        /// Sprawdza czy podkategoria istnieje i należy do określonej kategorii.
        /// Waliduje zarówno istnienie podkategorii jak i poprawność relacji CategoryId.
        /// Używane przy walidacji CreateContactCommand.
        /// </summary>
        public async Task<bool> ExistsForCategory(int subcategoryId, int categoryId)
        {
            return await _dbContext.Subcategories
                .AnyAsync(s => s.Id == subcategoryId && s.CategoryId == categoryId);
        }

        /// <summary>
        /// Pobiera wszystkie podkategorie należące do określonej kategorii.
        /// Używane do wyświetlenia listy wyboru w UI (np. dla kategorii "służbowy").
        /// </summary>
        public async Task<IEnumerable<Subcategory>> GetByCategoryId(int categoryId)
        {
            return await _dbContext.Subcategories
                .Where(s => s.CategoryId == categoryId)
                .ToListAsync();
        }
    }
}
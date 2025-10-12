using Microsoft.EntityFrameworkCore;
using NetPcContacts.Domain.Entities;
using NetPcContacts.Domain.IRepositories;
using NetPcContacts.Infrastructure.Persistence;

namespace NetPcContacts.Infrastructure.Repositories
{
    /// <summary>
    /// Implementacja repozytorium dla encji Category.
    /// Zapewnia dostęp do danych kategorii w bazie przy użyciu Entity Framework Core.
    /// </summary>
    internal class CategoryRepository : ICategoryRepository
    {
        private readonly NetPcContactsDbContext _dbContext;

        public CategoryRepository(NetPcContactsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Sprawdza czy kategoria o podanym ID istnieje w bazie.
        /// Używa AnyAsync dla wydajności (nie pobiera całej encji).
        /// </summary>
        public async Task<bool> Exists(int categoryId)
        {
            return await _dbContext.Categories
                .AnyAsync(c => c.Id == categoryId);
        }

        /// <summary>
        /// Pobiera kategorię wraz z powiązanymi podkategoriami.
        /// </summary>
        public async Task<Category?> GetByIdWithSubcategories(int categoryId)
        {
            return await _dbContext.Categories
                .Include(c => c.Subcategories)
                .FirstOrDefaultAsync(c => c.Id == categoryId);
        }

        /// <summary>
        /// Pobiera wszystkie kategorie z bazy.
        /// Używane do wyświetlenia listy wyboru w UI.
        /// </summary>
        public async Task<IEnumerable<Category>> GetAll()
        {
            return await _dbContext.Categories.ToListAsync();
        }
    }
}

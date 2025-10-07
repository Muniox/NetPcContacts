using NetPcContacts.Domain.Entities;
using NetPcContacts.Domain.IRepositories;
using NetPcContacts.Infrastructure.Persistence;

namespace NetPcContacts.Infrastructure.Repositories;

internal class SubcategoryRepository : ISubcategoryRepository
{
    private readonly NetPcContactsDbContext _dbContext;

    public SubcategoryRepository(NetPcContactsDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<int> Create(Subcategory subcategory)
    {
        _dbContext.Subcategories.Add(subcategory);
        await _dbContext.SaveChangesAsync();
        return subcategory.Id;
    }
}
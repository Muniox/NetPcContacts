using NetPcContacts.Domain.Entities;

namespace NetPcContacts.Domain.IRepositories;

public interface ISubcategoryRepository
{
    Task<int> Create(Subcategory subcategory);
}
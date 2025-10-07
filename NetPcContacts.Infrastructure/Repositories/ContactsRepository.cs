using Microsoft.EntityFrameworkCore;
using NetPcContacts.Domain.Entities;
using NetPcContacts.Domain.IRepositories;
using NetPcContacts.Infrastructure.Persistence;

namespace NetPcContacts.Infrastructure.Repositories
{
    internal class ContactsRepository : IContactsRepository
    {
        private readonly NetPcContactsDbContext _dbContext;
        public ContactsRepository(NetPcContactsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> Create(Contact contact)
        {
            _dbContext.Contacts.Add(contact);
            await _dbContext.SaveChangesAsync();
            return contact.Id;
        }

        public async Task Delete(Contact contact)
        {
            _dbContext.Contacts.Remove(contact);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Contact>> GetAll()
        {
            return await _dbContext.Contacts
                .Include(c => c.Category)
                .Include(c => c.Subcategory)
                .ToListAsync();
        }

        public async Task<Contact?> GetById(int id)
        {
            return await _dbContext.Contacts
                .Include(c => c.Category)
                .Include(c => c.Subcategory)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task SaveChanges()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}

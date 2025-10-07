using NetPcContacts.Domain.Entities;

namespace NetPcContacts.Domain.IRepositories

{
    public interface IContactsRepository
    {
        Task<int> Create(Contact contact);

        Task Delete(Contact contact);

        Task SaveChanges();

        Task<IEnumerable<Contact>> GetAll();

        Task<Contact?> GetById(int id);
    }
}

using Mediator;
using NetPcContacts.Application.Contacts.Dtos;

namespace NetPcContacts.Application.Contacts.Queries.GetContactById
{
    /// <summary>
    /// Query do pobrania pojedynczego kontaktu po ID.
    /// Zwraca ContactDto z pełnymi danymi kontaktu (bez hasła).
    /// </summary>
    public class GetContactByIdQuery(int contactId) : IQuery<ContactDto?>
    {
        /// <summary>
        /// ID kontaktu do pobrania.
        /// </summary>
        public int ContactId { get; set; } = contactId;
    }
}

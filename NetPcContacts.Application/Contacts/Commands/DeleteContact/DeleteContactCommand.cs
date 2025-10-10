using MediatR;

namespace NetPcContacts.Application.Contacts.Commands.DeleteContact
{
    /// <summary>
    /// Komenda do usuwania kontaktu.
    /// Implementuje wzorzec CQRS poprzez MediatR.
    /// Nie zwraca żadnej wartości (Unit).
    /// </summary>
    public class DeleteContactCommand(int contactId) : IRequest
    {
        /// <summary>
        /// ID kontaktu do usunięcia.
        /// </summary>
        public int ContactId { get; set; } = contactId;
    }
}
using Mediator;

namespace NetPcContacts.Application.Contacts.Commands.DeleteContact
{
    /// <summary>
    /// Komenda do usuwania kontaktu.
    /// Implementuje wzorzec CQRS poprzez Mediator.
    /// Nie zwraca żadnej wartości (Unit).
    /// </summary>
    public class DeleteContactCommand(int contactId) : ICommand
    {
        /// <summary>
        /// ID kontaktu do usunięcia.
        /// </summary>
        public int ContactId { get; set; } = contactId;
    }
}
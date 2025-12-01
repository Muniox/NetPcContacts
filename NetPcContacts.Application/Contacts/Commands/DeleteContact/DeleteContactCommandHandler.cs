using Mediator;
using Microsoft.Extensions.Logging;
using NetPcContacts.Domain.Exceptions;
using NetPcContacts.Domain.IRepositories;

namespace NetPcContacts.Application.Contacts.Commands.DeleteContact
{
    /// <summary>
    /// Handler obsługujący komendę usuwania kontaktu.
    /// Implementuje wzorzec CQRS poprzez Mediator.
    /// Sprawdza czy kontakt istnieje i usuwa go z bazy danych.
    /// </summary>
    public class DeleteContactCommandHandler : ICommandHandler<DeleteContactCommand>
    {
        private readonly IContactsRepository _contactsRepository;
        private readonly ILogger<DeleteContactCommandHandler> _logger;

        /// <summary>
        /// Konstruktor wstrzykujący zależności.
        /// </summary>
        /// <param name="contactsRepository">Repozytorium kontaktów</param>
        /// <param name="logger">Logger do rejestrowania operacji</param>
        public DeleteContactCommandHandler(
            IContactsRepository contactsRepository,
            ILogger<DeleteContactCommandHandler> logger)
        {
            _contactsRepository = contactsRepository;
            _logger = logger;
        }

        /// <summary>
        /// Obsługuje komendę DeleteContactCommand.
        /// Sprawdza czy kontakt istnieje i usuwa go z bazy.
        /// </summary>
        /// <param name="command">Komenda zawierająca ID kontaktu do usunięcia</param>
        /// <param name="cancellationToken">Token anulowania operacji</param>
        /// <exception cref="NotFoundException">Rzucany gdy kontakt o podanym ID nie istnieje</exception>
        public async ValueTask<Unit> Handle(DeleteContactCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting contact: {ContactId}", command.ContactId);

            // KROK 1: Sprawdź czy kontakt istnieje
            var contact = await _contactsRepository.GetById(command.ContactId);

            // KROK 2: Jeśli kontakt nie istnieje, rzuć wyjątek NotFoundException
            if (contact == null)
            {
                throw new NotFoundException(nameof(contact), command.ContactId.ToString());
            }

            // KROK 3: Usuń kontakt z bazy danych
            await _contactsRepository.Delete(contact);

            _logger.LogInformation("Contact deleted successfully: {ContactId}", command.ContactId);
            
            return Unit.Value;
        }
    }
}

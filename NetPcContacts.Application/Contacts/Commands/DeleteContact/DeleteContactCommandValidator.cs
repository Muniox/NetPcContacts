using FluentValidation;

namespace NetPcContacts.Application.Contacts.Commands.DeleteContact
{
    /// <summary>
    /// Validator dla komendy DeleteContactCommand.
    /// Implementuje reguły walidacji danych wejściowych.
    /// Używa biblioteki FluentValidation do deklaratywnej definicji reguł.
    /// </summary>
    public class DeleteContactCommandValidator : AbstractValidator<DeleteContactCommand>
    {
        public DeleteContactCommandValidator()
        {
            // Walidacja ContactId - wymagane, musi być większe od zera
            // Uwaga: Sprawdzenie czy kontakt istnieje odbywa się w handlerze
            RuleFor(x => x.ContactId)
                .GreaterThan(0)
                .WithMessage("ID kontaktu musi być większe od zera.");
        }
    }
}
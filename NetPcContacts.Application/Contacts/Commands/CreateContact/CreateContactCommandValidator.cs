using FluentValidation;

namespace NetPcContacts.Application.Contacts.Commands.CreateContact
{
    /// <summary>
    /// Validator dla komendy CreateContactCommand.
    /// Implementuje reguły walidacji danych wejściowych zgodnie z wymaganiami biznesowymi.
    /// Używa biblioteki FluentValidation do deklaratywnej definicji reguł.
    /// </summary>
    public class CreateContactCommandValidator : AbstractValidator<CreateContactCommand>
    {
        public CreateContactCommandValidator()
        {
            // Walidacja imienia - wymagane, niepuste, długość od 1 do 100 znaków
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Imię jest wymagane.")
                .Length(1, 100)
                .WithMessage("Imię musi zawierać od 1 do 100 znaków.");

            // Walidacja nazwiska - wymagane, niepuste, długość od 1 do 100 znaków
            RuleFor(x => x.Surname)
                .NotEmpty()
                .WithMessage("Nazwisko jest wymagane.")
                .Length(1, 100)
                .WithMessage("Nazwisko musi zawierać od 1 do 100 znaków.");

            // Walidacja emaila - wymagane, poprawny format, długość do 255 znaków
            // Uwaga: Unikalność emaila jest sprawdzana w handlerze, nie w validatorze
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email jest wymagany.")
                .EmailAddress()
                .WithMessage("Nieprawidłowy format adresu email.")
                .MaximumLength(255)
                .WithMessage("Email nie może przekraczać 255 znaków.");

            // Walidacja hasła - wymagane, spełniające standardy złożoności
            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Hasło jest wymagane.")
                .MinimumLength(8)
                .WithMessage("Hasło musi zawierać minimum 8 znaków.")
                .MaximumLength(100)
                .WithMessage("Hasło nie może przekraczać 100 znaków.")
                .Matches(@"[A-Z]")
                .WithMessage("Hasło musi zawierać przynajmniej jedną wielką literę.")
                .Matches(@"[a-z]")
                .WithMessage("Hasło musi zawierać przynajmniej jedną małą literę.")
                .Matches(@"[0-9]")
                .WithMessage("Hasło musi zawierać przynajmniej jedną cyfrę.")
                .Matches(@"[\W_]")
                .WithMessage("Hasło musi zawierać przynajmniej jeden znak specjalny.");

            // Walidacja numeru telefonu - wymagane, podstawowy format
            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage("Numer telefonu jest wymagany.")
                .Matches(@"^[\d\s\-\+\(\)]+$")
                .WithMessage("Numer telefonu może zawierać tylko cyfry, spacje, myślniki, plus i nawiasy.")
                .Length(9, 20)
                .WithMessage("Numer telefonu musi zawierać od 9 do 20 znaków.");

            // Walidacja daty urodzenia - musi być datą z przeszłości, rozsądny przedział wiekowy
            RuleFor(x => x.BirthDate)
                .NotEmpty()
                .WithMessage("Data urodzenia jest wymagana.")
                .LessThan(DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("Data urodzenia musi być datą z przeszłości.")
                .GreaterThan(DateOnly.FromDateTime(DateTime.Today.AddYears(-150)))
                .WithMessage("Data urodzenia nie może być starsza niż 150 lat.");

            // Walidacja CategoryId - wymagane, większe od zera
            // Uwaga: Sprawdzenie czy kategoria istnieje odbywa się w handlerze
            RuleFor(x => x.CategoryId)
                .GreaterThan(0)
                .WithMessage("Kategoria jest wymagana.");

            // Walidacja SubcategoryId - jeśli podane, musi być większe od zera
            RuleFor(x => x.SubcategoryId)
                .GreaterThan(0)
                .When(x => x.SubcategoryId.HasValue)
                .WithMessage("Podkategoria musi mieć prawidłową wartość.");

            // Walidacja CustomSubcategory - jeśli podane, nie może być puste i ma limit znaków
            RuleFor(x => x.CustomSubcategory)
                .NotEmpty()
                .When(x => !string.IsNullOrWhiteSpace(x.CustomSubcategory))
                .WithMessage("Niestandardowa podkategoria nie może być pusta.")
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.CustomSubcategory))
                .WithMessage("Niestandardowa podkategoria nie może przekraczać 100 znaków.");
        }
    }
}

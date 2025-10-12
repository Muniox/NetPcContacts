using FluentValidation;

namespace NetPcContacts.Application.Contacts.Commands.UpdateContact
{
    /// <summary>
    /// Validator dla komendy UpdateContactCommand.
    /// Implementuje reguły walidacji danych wejściowych zgodnie z wymaganiami biznesowymi.
    /// Używa biblioteki FluentValidation do deklaratywnej definicji reguł.
    /// </summary>
    public class UpdateContactCommandValidator : AbstractValidator<UpdateContactCommand>
    {
        public UpdateContactCommandValidator()
        {
            // Walidacja ID kontaktu - wymagane, większe od zera
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("ID kontaktu musi być większe od zera.");

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

            // Walidacja hasła - opcjonalne, ale jeśli podane, musi spełniać standardy złożoności
            // Jeśli hasło jest null lub puste, walidacja nie jest wykonywana (hasło nie zostanie zmienione)
            RuleFor(x => x.Password)
                .MinimumLength(8)
                .When(x => !string.IsNullOrWhiteSpace(x.Password))
                .WithMessage("Hasło musi zawierać minimum 8 znaków.")
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.Password))
                .WithMessage("Hasło nie może przekraczać 100 znaków.")
                .Matches(@"[A-Z]")
                .When(x => !string.IsNullOrWhiteSpace(x.Password))
                .WithMessage("Hasło musi zawierać przynajmniej jedną wielką literę.")
                .Matches(@"[a-z]")
                .When(x => !string.IsNullOrWhiteSpace(x.Password))
                .WithMessage("Hasło musi zawierać przynajmniej jedną małą literę.")
                .Matches(@"[0-9]")
                .When(x => !string.IsNullOrWhiteSpace(x.Password))
                .WithMessage("Hasło musi zawierać przynajmniej jedną cyfrę.")
                .Matches(@"[\W_]")
                .When(x => !string.IsNullOrWhiteSpace(x.Password))
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

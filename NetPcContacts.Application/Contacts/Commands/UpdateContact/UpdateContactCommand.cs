using MediatR;

namespace NetPcContacts.Application.Contacts.Commands.UpdateContact
{
    /// <summary>
    /// Komenda do aktualizacji istniejącego kontaktu.
    /// Implementuje wzorzec CQRS poprzez MediatR.
    /// Nie zwraca żadnej wartości (Unit).
    /// </summary>
    public class UpdateContactCommand : IRequest
    {
        /// <summary>
        /// ID kontaktu do aktualizacji.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Imię kontaktu - wymagane pole tekstowe.
        /// </summary>
        public string Name { get; set; } = default!;

        /// <summary>
        /// Nazwisko kontaktu - wymagane pole tekstowe.
        /// </summary>
        public string Surname { get; set; } = default!;

        /// <summary>
        /// Adres email kontaktu - musi być unikalny w systemie.
        /// Walidowany pod kątem poprawności formatu oraz unikalności.
        /// </summary>
        public string Email { get; set; } = default!;

        /// <summary>
        /// Hasło kontaktu - opcjonalne przy aktualizacji.
        /// Jeśli podane, musi spełniać standardy złożoności hasła.
        /// Jeśli null lub puste, hasło nie zostanie zmienione.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Numer telefonu kontaktu.
        /// Walidowany pod kątem poprawności formatu.
        /// </summary>
        public string PhoneNumber { get; set; } = default!;

        /// <summary>
        /// Data urodzenia kontaktu.
        /// Musi być datą z przeszłości.
        /// </summary>
        public DateOnly BirthDate { get; set; }

        /// <summary>
        /// ID kategorii kontaktu ze słownika w bazie danych.
        /// Możliwe wartości pobierane z tabeli Categories (np. służbowy, prywatny, inny).
        /// Walidowane względem istniejących rekordów w bazie.
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// ID podkategorii kontaktu ze słownika w bazie danych - pole opcjonalne.
        /// Używane dla kategorii "służbowy" - wybór ze słownika (np. szef, klient, dostawca).
        /// Dla kategorii "prywatny" - zazwyczaj null.
        /// Dla kategorii "inny" - null, zamiast tego używane jest CustomSubcategory.
        /// Walidowane względem istniejących rekordów powiązanych z daną kategorią.
        /// </summary>
        public int? SubcategoryId { get; set; }

        /// <summary>
        /// Niestandardowa nazwa podkategorii wpisana bezpośrednio przez użytkownika.
        /// Używana wyłącznie gdy CategoryId wskazuje na kategorię typu "inny".
        /// Ignorowana dla innych typów kategorii.
        /// </summary>
        public string? CustomSubcategory { get; set; }
    }
}

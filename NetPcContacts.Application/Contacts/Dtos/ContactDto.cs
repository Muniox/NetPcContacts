namespace NetPcContacts.Application.Contacts.Dtos
{
    /// <summary>
    /// Data Transfer Object reprezentujący dane kontaktu zwracane z zapytań.
    /// NIE zawiera wrażliwych informacji jak PasswordHash.
    /// </summary>
    public class ContactDto
    {
        /// <summary>
        /// Unikalny identyfikator kontaktu.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Imię kontaktu.
        /// </summary>
        public string Name { get; set; } = default!;

        /// <summary>
        /// Nazwisko kontaktu.
        /// </summary>
        public string Surname { get; set; } = default!;

        /// <summary>
        /// Adres email kontaktu.
        /// </summary>
        public string Email { get; set; } = default!;

        /// <summary>
        /// Numer telefonu kontaktu.
        /// </summary>
        public string PhoneNumber { get; set; } = default!;

        /// <summary>
        /// Data urodzenia kontaktu.
        /// </summary>
        public DateOnly BirthDate { get; set; }

        /// <summary>
        /// ID kategorii kontaktu.
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Nazwa kategorii kontaktu (np. "Służbowy", "Prywatny", "Inny").
        /// </summary>
        public string CategoryName { get; set; } = default!;

        /// <summary>
        /// ID podkategorii kontaktu (opcjonalne).
        /// Null gdy kategoria nie ma podkategorii lub używany jest CustomSubcategory.
        /// </summary>
        public int? SubcategoryId { get; set; }

        /// <summary>
        /// Nazwa podkategorii ze słownika (opcjonalna).
        /// Np. "Szef", "Klient" dla kategorii "Służbowy".
        /// </summary>
        public string? SubcategoryName { get; set; }

        /// <summary>
        /// Niestandardowa podkategoria wpisana przez użytkownika.
        /// Używana dla kategorii "Inny".
        /// </summary>
        public string? CustomSubcategory { get; set; }
    }
}

namespace NetPcContacts.Application.Contacts.Dtos
{
    /// <summary>
    /// DTO zawierające podstawowe dane kontaktu do wyświetlenia na liście.
    /// </summary>
    public class BasicContactDto
    {
        /// <summary>
        /// Unikalny identyfikator kontaktu.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Imię kontaktu.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Nazwisko kontaktu.
        /// </summary>
        public string Surname { get; set; } = string.Empty;

        /// <summary>
        /// Adres email kontaktu.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Numer telefonu kontaktu.
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Kategoria kontaktu (służbowy, prywatny, inny).
        /// </summary>
        public string Category { get; set; } = string.Empty;
    }
}

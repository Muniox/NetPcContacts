namespace NetPcContacts.Domain.Entities
{
    /// <summary>
    /// Encja reprezentująca kontakt w systemie.
    /// Przechowuje dane osobowe, uwierzytelniające oraz kategoryzację kontaktu.
    /// </summary>
    public class Contact
    {
        /// <summary>
        /// Unikalny identyfikator kontaktu w bazie danych.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Imię kontaktu.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Nazwisko kontaktu.
        /// </summary>
        public string Surname { get; set; }
        
        /// <summary>
        /// Adres email kontaktu - unikalny w systemie.
        /// Używany jako identyfikator przy logowaniu.
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// Zahashowane hasło kontaktu.
        /// Nigdy nie przechowujemy hasła w postaci jawnej - tylko hash utworzony przez IPasswordHasher.
        /// </summary>
        public string PasswordHash { get; set; }
        
        /// <summary>
        /// Numer telefonu kontaktu.
        /// </summary>
        public string PhoneNumber { get; set; }
        
        /// <summary>
        /// Data urodzenia kontaktu.
        /// </summary>
        public DateOnly BirthDate { get; set; }

        // Nawigacyjne właściwości dla Category
        
        /// <summary>
        /// Klucz obcy do tabeli Categories.
        /// Określa główną kategorię kontaktu (służbowy/prywatny/inny).
        /// </summary>
        public int CategoryId { get; set; }
        
        /// <summary>
        /// Nawigacyjna właściwość do encji Category.
        /// </summary>
        public Category Category { get; set; }

        // Nawigacyjne właściwości dla Subcategory
        
        /// <summary>
        /// Klucz obcy do tabeli Subcategories (opcjonalny).
        /// Używany dla kategorii "służbowy" - wybór ze słownika (szef, klient, etc.).
        /// Null dla kategorii "prywatny" i "inny".
        /// </summary>
        public int? SubcategoryId { get; set; }
        
        /// <summary>
        /// Nawigacyjna właściwość do encji Subcategory.
        /// </summary>
        public Subcategory? Subcategory { get; set; }
        
        /// <summary>
        /// Niestandardowa podkategoria wpisana przez użytkownika.
        /// Używana wyłącznie gdy Category.CategoryName = "Inny".
        /// Pozwala użytkownikowi na wpisanie dowolnej wartości zamiast wyboru ze słownika.
        /// </summary>
        public string? CustomSubcategory { get; set; }
    }
}

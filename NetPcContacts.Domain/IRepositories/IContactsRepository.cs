using NetPcContacts.Domain.Constants;
using NetPcContacts.Domain.Entities;

namespace NetPcContacts.Domain.IRepositories
{
    /// <summary>
    /// Interfejs repozytorium dla encji Contact.
    /// Definiuje operacje CRUD oraz dodatkowe metody pomocnicze.
    /// </summary>
    public interface IContactsRepository
    {
        /// <summary>
        /// Tworzy nowy kontakt w bazie danych.
        /// </summary>
        /// <param name="contact">Encja kontaktu do utworzenia</param>
        /// <returns>ID utworzonego kontaktu</returns>
        Task<int> Create(Contact contact);

        /// <summary>
        /// Usuwa kontakt z bazy danych.
        /// </summary>
        /// <param name="contact">Encja kontaktu do usunięcia</param>
        Task Delete(Contact contact);

        /// <summary>
        /// Pobiera wszystkie kontakty wraz z ich kategoriami i podkategoriami.
        /// </summary>
        /// <returns>Kolekcja wszystkich kontaktów</returns>
        Task<IEnumerable<Contact>> GetAll();

        /// <summary>
        /// Pobiera kontakt po ID wraz z powiązanymi encjami (Category, Subcategory).
        /// </summary>
        /// <param name="id">ID kontaktu</param>
        /// <returns>Encja Contact lub null jeśli nie znaleziono</returns>
        Task<Contact?> GetById(int id);

        /// <summary>
        /// Pobiera kontakt po adresie email.
        /// Używane do sprawdzania unikalności emaila oraz podczas logowania.
        /// </summary>
        /// <param name="email">Adres email kontaktu</param>
        /// <returns>Encja Contact lub null jeśli nie znaleziono</returns>
        Task<Contact?> GetByEmail(string email);

        /// <summary>
        /// Sprawdza czy kontakt o podanym emailu już istnieje w bazie.
        /// Używane do walidacji podczas tworzenia nowego kontaktu.
        /// </summary>
        /// <param name="email">Adres email do sprawdzenia</param>
        /// <returns>True jeśli email już istnieje, false w przeciwnym razie</returns>
        Task<bool> EmailExists(string email);

        /// <summary>
        /// Zapisuje wszystkie zmiany w kontekście do bazy danych.
        /// Używane po modyfikacji istniejącej encji.
        /// </summary>
        Task SaveChanges();

        /// <summary>
        /// Pobiera stronicowaną listę kontaktów spełniających kryteria wyszukiwania z możliwością sortowania.
        /// </summary>
        /// <param name="searchPhrase">Fraza wyszukiwania - filtruje kontakty po imieniu, nazwisku lub emailu (opcjonalne)</param>
        /// <param name="pageSize">Liczba kontaktów na stronę</param>
        /// <param name="pageNumber">Numer strony (numeracja od 1)</param>
        /// <param name="sortBy">Nazwa pola do sortowania (opcjonalne)</param>
        /// <param name="sortDirection">Kierunek sortowania (rosnąco/malejąco)</param>
        /// <returns>Krotka zawierająca kolekcję kontaktów na danej stronie oraz całkowitą liczbę kontaktów spełniających kryteria</returns>
        Task<(IEnumerable<Contact>, int)> GetAllMatchingAsync(string? searchPhrase, int pageSize, int pageNumber, string? sortBy, SortDirection sortDirection);
    }
}


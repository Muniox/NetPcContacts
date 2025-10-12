using NetPcContacts.Domain.Entities;

namespace NetPcContacts.Domain.IRepositories
{
    /// <summary>
    /// Interfejs repozytorium dla encji Category.
    /// Definiuje operacje dostępu do danych kategorii w bazie.
    /// </summary>
    public interface ICategoryRepository
    {
        /// <summary>
        /// Sprawdza czy kategoria o podanym ID istnieje w bazie danych.
        /// Używane do walidacji CategoryId przed utworzeniem kontaktu.
        /// </summary>
        /// <param name="categoryId">ID kategorii do sprawdzenia</param>
        /// <returns>True jeśli kategoria istnieje, false w przeciwnym razie</returns>
        Task<bool> Exists(int categoryId);

        /// <summary>
        /// Pobiera kategorię po ID wraz z jej podkategoriami.
        /// </summary>
        /// <param name="categoryId">ID kategorii</param>
        /// <returns>Encja Category lub null jeśli nie znaleziono</returns>
        Task<Category?> GetByIdWithSubcategories(int categoryId);

        /// <summary>
        /// Pobiera wszystkie kategorie z bazy danych.
        /// Używane do wyświetlenia listy kategorii w UI.
        /// </summary>
        /// <returns>Kolekcja wszystkich kategorii</returns>
        Task<IEnumerable<Category>> GetAll();
    }
}

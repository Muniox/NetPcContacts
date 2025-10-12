using NetPcContacts.Domain.Entities;

namespace NetPcContacts.Domain.IRepositories
{
    /// <summary>
    /// Interfejs repozytorium dla encji Subcategory.
    /// Definiuje operacje dostępu do danych podkategorii w bazie.
    /// </summary>
    public interface ISubcategoryRepository
    {
        /// <summary>
        /// Tworzy nową podkategorię w bazie danych.
        /// </summary>
        /// <param name="subcategory">Encja podkategorii do utworzenia</param>
        /// <returns>ID utworzonej podkategorii</returns>
        Task<int> Create(Subcategory subcategory);

        /// <summary>
        /// Sprawdza czy podkategoria o podanym ID istnieje i należy do określonej kategorii.
        /// Używane do walidacji SubcategoryId przy tworzeniu kontaktu.
        /// </summary>
        /// <param name="subcategoryId">ID podkategorii</param>
        /// <param name="categoryId">ID kategorii nadrzędnej</param>
        /// <returns>True jeśli podkategoria istnieje i należy do kategorii, false w przeciwnym razie</returns>
        Task<bool> ExistsForCategory(int subcategoryId, int categoryId);

        /// <summary>
        /// Pobiera wszystkie podkategorie należące do określonej kategorii.
        /// </summary>
        /// <param name="categoryId">ID kategorii nadrzędnej</param>
        /// <returns>Kolekcja podkategorii</returns>
        Task<IEnumerable<Subcategory>> GetByCategoryId(int categoryId);
    }
}
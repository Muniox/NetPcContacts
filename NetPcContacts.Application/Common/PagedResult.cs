namespace NetPcContacts.Application.Common
{
    /// <summary>
    /// Reprezentuje wynik zapytania z paginacją.
    /// </summary>
    /// <typeparam name="T">Typ elementów zwracanych na stronie.</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// Inicjalizuje nową instancję klasy PageResult.
        /// </summary>
        /// <param name="items">Kolekcja elementów na aktualnej stronie.</param>
        /// <param name="totalCount">Całkowita liczba elementów w zbiorze danych.</param>
        /// <param name="pageSize">Liczba elementów na jednej stronie.</param>
        /// <param name="pageNumber">Numer aktualnej strony (numeracja od 1).</param>
        public PagedResult(IEnumerable<T> items, int totalCount, int pageSize, int pageNumber)
        {
            Items = items;
            TotalItemsCount = totalCount;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ItemsFrom = pageSize * (pageNumber - 1) + 1;
            ItemsTo = ItemsFrom + pageSize - 1;
        }

        /// <summary>
        /// Kolekcja elementów na aktualnej stronie.
        /// </summary>
        public IEnumerable<T> Items { get; set; }

        /// <summary>
        /// Całkowita liczba stron.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Całkowita liczba elementów w zbiorze danych.
        /// </summary>
        public int TotalItemsCount { get; set; }

        /// <summary>
        /// Indeks pierwszego elementu na aktualnej stronie (numeracja od 1).
        /// </summary>
        public int ItemsFrom { get; set; }

        /// <summary>
        /// Indeks ostatniego elementu na aktualnej stronie (numeracja od 1).
        /// </summary>
        public int ItemsTo { get; set; }
    }
}

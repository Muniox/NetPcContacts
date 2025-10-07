namespace NetPcContacts.Domain.Entities
{
    public class Category 
    { 
        public int Id { get; set; }

        /// <summary>
        /// Nazwa kategorii ("Służbowy", "Prywatny", "Inny")
        /// </summary>
        public string CategoryName { get; set; }
        
        // Nawigacyjne właściwości (jeden-do-wielu)
        public ICollection<Subcategory> Subcategories { get; set; } = new List<Subcategory>();
        public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    }
}

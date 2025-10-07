namespace NetPcContacts.Domain.Entities
{
    public class Subcategory
    {
        public int Id { get; set; }
        public string SubcategoryName { get; set; }

        // Nawigacyjne właściwości dla Category (jeden-do-wielu)
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        // Nawigacyjne właściwości dla Contact
        public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    }
}

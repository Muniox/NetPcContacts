namespace NetPcContacts.Domain.Entities
{
    public class Contact
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public DateOnly BirthDate { get; set; }

        // Nawigacyjne właściwości dla Category
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        // Nawigacyjne właściwości dla Subcategory
        public int? SubcategoryId { get; set; }
        public Subcategory? Subcategory { get; set; }
    }
}

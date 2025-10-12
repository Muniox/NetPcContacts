namespace NetPcContacts.Application.Contacts.Dtos
{
    /// <summary>
    /// DTO do aktualizacji kontaktu (używany w API request body).
    /// Nie zawiera pola Id, które jest przekazywane przez route parameter.
    /// </summary>
    public class UpdateContactDto
    {
        public string Name { get; set; } = default!;
        public string Surname { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? Password { get; set; }
        public string PhoneNumber { get; set; } = default!;
        public DateOnly BirthDate { get; set; }
        public int CategoryId { get; set; }
        public int? SubcategoryId { get; set; }
        public string? CustomSubcategory { get; set; }
    }
}

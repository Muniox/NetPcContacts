using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NetPcContacts.Domain.Entities;

namespace NetPcContacts.Infrastructure.Persistence
{
    /// <summary>
    /// Kontekst bazy danych dla aplikacji NetPcContacts.
    /// Dziedziczy po IdentityDbContext dla obsługi autentykacji i autoryzacji użytkowników.
    /// </summary>
    internal class NetPcContactsDbContext : IdentityDbContext<User>
    {
        /// <summary>
        /// Zbiór encji Contact w bazie danych.
        /// </summary>
        internal DbSet<Contact> Contacts { get; set; }
        
        /// <summary>
        /// Zbiór encji Category w bazie danych (słownik kategorii).
        /// </summary>
        internal DbSet<Category> Categories { get; set; }
        
        /// <summary>
        /// Zbiór encji Subcategory w bazie danych (słownik podkategorii).
        /// </summary>
        internal DbSet<Subcategory> Subcategories { get; set; }

        public NetPcContactsDbContext(DbContextOptions<NetPcContactsDbContext> options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========================================
            // KONFIGURACJA ENCJI CONTACT
            // ========================================

            // Konfiguracja właściwości Contact.Name
            // Zgodnie z CreateContactCommandValidator: Length(1, 100)
            modelBuilder.Entity<Contact>()
                .Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Konfiguracja właściwości Contact.Surname
            // Zgodnie z CreateContactCommandValidator: Length(1, 100)
            modelBuilder.Entity<Contact>()
                .Property(c => c.Surname)
                .IsRequired()
                .HasMaxLength(100);

            // Konfiguracja właściwości Contact.Email
            // Zgodnie z CreateContactCommandValidator: MaximumLength(255)
            // WYMAGANIE: Email musi być unikalny w całym systemie
            modelBuilder.Entity<Contact>()
                .Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<Contact>()
                .HasIndex(c => c.Email)
                .IsUnique();

            // Konfiguracja właściwości Contact.PasswordHash
            // Hash hasła może być długi (PBKDF2 generuje długie hashe)
            // Standardowy hash z Identity to ~100-150 znaków
            modelBuilder.Entity<Contact>()
                .Property(c => c.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            // Konfiguracja właściwości Contact.PhoneNumber
            // Zgodnie z CreateContactCommandValidator: Length(9, 20)
            modelBuilder.Entity<Contact>()
                .Property(c => c.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            // Konfiguracja właściwości Contact.CustomSubcategory
            // Zgodnie z CreateContactCommandValidator: MaximumLength(100)
            // Pole opcjonalne
            modelBuilder.Entity<Contact>()
                .Property(c => c.CustomSubcategory)
                .HasMaxLength(100);

            // ========================================
            // KONFIGURACJA ENCJI CATEGORY
            // ========================================

            // Konfiguracja właściwości Category.CategoryName
            // Przykładowe wartości: "Służbowy", "Prywatny", "Inny"
            // Limit 50 znaków wystarczy dla nazw kategorii
            modelBuilder.Entity<Category>()
                .Property(c => c.CategoryName)
                .IsRequired()
                .HasMaxLength(50);

            // ========================================
            // KONFIGURACJA ENCJI SUBCATEGORY
            // ========================================

            // Konfiguracja właściwości Subcategory.SubcategoryName
            // Przykładowe wartości: "Szef", "Klient", "Dostawca", itp.
            // Limit 100 znaków dla nazw podkategorii
            modelBuilder.Entity<Subcategory>()
                .Property(s => s.SubcategoryName)
                .IsRequired()
                .HasMaxLength(100);

            // ========================================
            // KONFIGURACJA RELACJI
            // ========================================

            // Konfiguracja relacji Contact -> Category (wiele do jednego)
            modelBuilder.Entity<Contact>()
                .HasOne(c => c.Category)
                .WithMany(cat => cat.Contacts)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Konfiguracja relacji Contact -> Subcategory (wiele do jednego, opcjonalna)
            modelBuilder.Entity<Contact>()
                .HasOne(c => c.Subcategory)
                .WithMany(sub => sub.Contacts)
                .HasForeignKey(c => c.SubcategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Konfiguracja relacji Category -> Subcategory (jeden do wielu)
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Subcategories)
                .WithOne(sub => sub.Category)
                .HasForeignKey(sub => sub.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
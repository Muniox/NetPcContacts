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

            // WYMAGANIE: Email musi być unikalny w całym systemie
            // Zapobiega duplikacji adresów email przy tworzeniu kont
            modelBuilder.Entity<Contact>()
                .HasIndex(c => c.Email)
                .IsUnique();
        }
    }
}
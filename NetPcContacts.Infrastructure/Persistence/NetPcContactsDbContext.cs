using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NetPcContacts.Domain.Entities;

namespace NetPcContacts.Infrastructure.Persistence;

internal class NetPcContactsDbContext : IdentityDbContext<User>
{
    internal DbSet<Contact> Contacts {get; set; }
    internal DbSet<Category> Categories { get; set; }
    internal DbSet<Subcategory> Subcategories { get; set; }

    public NetPcContactsDbContext(DbContextOptions<NetPcContactsDbContext> options) : base(options)
    {}
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Contact>()
            .HasOne(c => c.Category)
            .WithMany(cat => cat.Contacts)
            .HasForeignKey(c => c.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Contact>()
            .HasOne(c => c.Subcategory)
            .WithMany(sub => sub.Contacts)
            .HasForeignKey(c => c.SubcategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Category>()
            .HasMany(c => c.Subcategories)
            .WithOne(sub => sub.Category)
            .HasForeignKey(sub => sub.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

    }
}
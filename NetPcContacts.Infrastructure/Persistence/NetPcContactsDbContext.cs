using Microsoft.EntityFrameworkCore;

namespace NetPcContacts.Infrastructure.Persistence;

internal class NetPcContactsDbContext : DbContext
{
    public NetPcContactsDbContext(DbContextOptions<NetPcContactsDbContext> options) : base(options)
    {
        
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
    }
}
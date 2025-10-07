using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using NetPcContacts.Domain.IRepositories;
using NetPcContacts.Infrastructure.Persistence;
using NetPcContacts.Infrastructure.Repositories;

namespace NetPcContacts.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("NetPcContactsDb");
        services.AddDbContext<NetPcContactsDbContext>(options =>
        {
            options.UseSqlite(connectionString);
            // .EnableSensitiveDataLogging();
        });

        // Rejestracja repozytorium
        services.AddScoped<IContactsRepository, ContactsRepository>();
    }
}
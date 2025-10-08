using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using NetPcContacts.Domain.IRepositories;
using NetPcContacts.Infrastructure.Persistence;
using NetPcContacts.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using NetPcContacts.Domain.Entities;

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

        services.AddIdentityCore<User>()
            .AddEntityFrameworkStores<NetPcContactsDbContext>()
            .AddApiEndpoints();

        // Rejestracja repozytorium
        services.AddScoped<IContactsRepository, ContactsRepository>();
    }
}
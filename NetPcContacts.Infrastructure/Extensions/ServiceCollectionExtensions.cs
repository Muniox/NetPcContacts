using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetPcContacts.Domain.Entities;
using NetPcContacts.Domain.IRepositories;
using NetPcContacts.Infrastructure.Persistence;
using NetPcContacts.Infrastructure.Repositories;
using NetPcContacts.Infrastructure.Seeders;

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

        // domyślnie token wygasa po 1h, zmieniamy na 1 min
        services.ConfigureAll<BearerTokenOptions>(option => option.BearerTokenExpiration = TimeSpan.FromMinutes(1));

        // Rejestracja repozytorium
        services.AddScoped<IContactsRepository, ContactsRepository>();
        
        // Rejestracja seedera
        services.AddScoped<IApplicationSeeder, ApplicationSeeder>();
    }
}
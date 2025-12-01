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

        // Identity Core - tylko persistence (bez API endpoints)
        services.AddIdentityCore<User>()
            .AddEntityFrameworkStores<NetPcContactsDbContext>();

        // Rejestracja repozytorium
        services.AddScoped<IContactsRepository, ContactsRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ISubcategoryRepository, SubcategoryRepository>();

        // Rejestracja seedera
        services.AddScoped<IApplicationSeeder, ApplicationSeeder>();

        // Rejestracja Password Hasher z ASP.NET Core Identity
        services.AddScoped<IPasswordHasher<Contact>, PasswordHasher<Contact>>();
    }
}
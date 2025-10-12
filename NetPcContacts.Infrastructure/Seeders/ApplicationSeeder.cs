using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NetPcContacts.Domain.Entities;
using NetPcContacts.Infrastructure.Persistence;

namespace NetPcContacts.Infrastructure.Seeders
{
    internal class ApplicationSeeder : IApplicationSeeder
    {
        private readonly NetPcContactsDbContext _dbContext;
        private readonly UserManager<User> _userManager;

        public ApplicationSeeder(NetPcContactsDbContext dbContext, UserManager<User> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task Seed()
        {
            if (_dbContext.Database.GetPendingMigrations().Any())
            {
                await _dbContext.Database.MigrateAsync();
            }

            if (await _dbContext.Database.CanConnectAsync())
            {
                if(!_dbContext.Categories.Any())
                {
                    var categories = GetCategories();
                    _dbContext.Categories.AddRange(categories);
                    await _dbContext.SaveChangesAsync();
                }

                if(!_dbContext.Users.Any())
                {
                    var user = new User()
                    {
                        Email = "test@gmail.com",
                        UserName = "test@gmail.com",
                        EmailConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(user, "@Test1234");
                    
                    if (!result.Succeeded)
                    {
                        throw new InvalidOperationException($"Nie udało się utworzyć użytkownika: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }

        private IEnumerable<Category> GetCategories()
        {
            List<Category> categories = new List<Category>()
            {
                new Category() 
                { 
                    CategoryName = "Służbowy",
                    Subcategories = new List<Subcategory>()
                    {
                        new Subcategory() { SubcategoryName = "szef" },
                        new Subcategory() { SubcategoryName = "współpracownik" },
                        new Subcategory() { SubcategoryName = "klient" }
                    }
                },
                new Category() { CategoryName = "Prywatny" },
                new Category() { CategoryName = "Inny" }
            };

            return categories;
        }
    }
}

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NetPcContacts.Domain.Entities;
using NetPcContacts.Infrastructure.Persistence;
using NetPcContacts.Infrastructure.Repositories;

namespace NetPcContacts.Infrastructure.Tests.Repositories
{
    /// <summary>
    /// Testy jednostkowe dla CategoryRepository.
    /// Używają InMemory Database do testowania operacji na kategoriach.
    /// </summary>
    public class CategoryRepositoryTests : IDisposable
    {
        private readonly NetPcContactsDbContext _dbContext;
        private readonly CategoryRepository _repository;

        public CategoryRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<NetPcContactsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new NetPcContactsDbContext(options);
            _repository = new CategoryRepository(_dbContext);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        #region Exists Tests

        [Fact]
        public async Task Exists_ForExistingCategory_ReturnsTrue()
        {
            // Arrange
            var category = new Category { CategoryName = "Służbowy" };
            await _dbContext.Categories.AddAsync(category);
            await _dbContext.SaveChangesAsync();

            // Act
            var exists = await _repository.Exists(category.Id);

            // Assert
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task Exists_ForNonExistingCategory_ReturnsFalse()
        {
            // Act
            var exists = await _repository.Exists(999);

            // Assert
            exists.Should().BeFalse();
        }

        #endregion

        #region GetAll Tests

        [Fact]
        public async Task GetAll_ReturnsAllCategories()
        {
            // Arrange
            var categories = new[]
            {
                new Category { CategoryName = "Służbowy" },
                new Category { CategoryName = "Prywatny" },
                new Category { CategoryName = "Inny" }
            };
            await _dbContext.Categories.AddRangeAsync(categories);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetAll();

            // Assert
            result.Should().HaveCount(3);
            result.Should().Contain(c => c.CategoryName == "Służbowy");
            result.Should().Contain(c => c.CategoryName == "Prywatny");
            result.Should().Contain(c => c.CategoryName == "Inny");
        }

        [Fact]
        public async Task GetAll_WhenNoCategories_ReturnsEmptyList()
        {
            // Act
            var result = await _repository.GetAll();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetByIdWithSubcategories Tests

        [Fact]
        public async Task GetByIdWithSubcategories_ForExistingCategory_ReturnsCategoryWithSubcategories()
        {
            // Arrange
            var category = new Category { CategoryName = "Służbowy" };
            await _dbContext.Categories.AddAsync(category);
            await _dbContext.SaveChangesAsync();

            var subcategories = new[]
            {
                new Subcategory { SubcategoryName = "Szef", CategoryId = category.Id },
                new Subcategory { SubcategoryName = "Klient", CategoryId = category.Id },
                new Subcategory { SubcategoryName = "Dostawca", CategoryId = category.Id }
            };
            await _dbContext.Subcategories.AddRangeAsync(subcategories);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdWithSubcategories(category.Id);

            // Assert
            result.Should().NotBeNull();
            result!.CategoryName.Should().Be("Służbowy");
            result.Subcategories.Should().HaveCount(3);
            result.Subcategories.Should().Contain(s => s.SubcategoryName == "Szef");
            result.Subcategories.Should().Contain(s => s.SubcategoryName == "Klient");
            result.Subcategories.Should().Contain(s => s.SubcategoryName == "Dostawca");
        }

        [Fact]
        public async Task GetByIdWithSubcategories_ForCategoryWithoutSubcategories_ReturnsCategoryWithEmptyList()
        {
            // Arrange
            var category = new Category { CategoryName = "Prywatny" };
            await _dbContext.Categories.AddAsync(category);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdWithSubcategories(category.Id);

            // Assert
            result.Should().NotBeNull();
            result!.CategoryName.Should().Be("Prywatny");
            result.Subcategories.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdWithSubcategories_ForNonExistingCategory_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByIdWithSubcategories(999);

            // Assert
            result.Should().BeNull();
        }

        #endregion
    }
}
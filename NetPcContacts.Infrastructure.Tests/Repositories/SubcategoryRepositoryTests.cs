using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NetPcContacts.Domain.Entities;
using NetPcContacts.Infrastructure.Persistence;
using NetPcContacts.Infrastructure.Repositories;

namespace NetPcContacts.Infrastructure.Tests.Repositories
{
    /// <summary>
    /// Testy jednostkowe dla SubcategoryRepository.
    /// Używają InMemory Database do testowania operacji na podkategoriach.
    /// </summary>
    public class SubcategoryRepositoryTests : IDisposable
    {
        private readonly NetPcContactsDbContext _dbContext;
        private readonly SubcategoryRepository _repository;

        public SubcategoryRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<NetPcContactsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new NetPcContactsDbContext(options);
            _repository = new SubcategoryRepository(_dbContext);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        #region Create Tests

        [Fact]
        public async Task Create_AddsSubcategoryToDatabase_ReturnsGeneratedId()
        {
            // Arrange
            var category = new Category { CategoryName = "Służbowy" };
            await _dbContext.Categories.AddAsync(category);
            await _dbContext.SaveChangesAsync();

            var subcategory = new Subcategory
            {
                SubcategoryName = "Szef",
                CategoryId = category.Id
            };

            // Act
            var subcategoryId = await _repository.Create(subcategory);

            // Assert
            subcategoryId.Should().BeGreaterThan(0);
            var savedSubcategory = await _dbContext.Subcategories.FindAsync(subcategoryId);
            savedSubcategory.Should().NotBeNull();
            savedSubcategory!.SubcategoryName.Should().Be("Szef");
            savedSubcategory.CategoryId.Should().Be(category.Id);
        }

        #endregion

        #region ExistsForCategory Tests

        [Fact]
        public async Task ExistsForCategory_ForValidSubcategoryAndCategory_ReturnsTrue()
        {
            // Arrange
            var category = new Category { CategoryName = "Służbowy" };
            await _dbContext.Categories.AddAsync(category);
            await _dbContext.SaveChangesAsync();

            var subcategory = new Subcategory
            {
                SubcategoryName = "Klient",
                CategoryId = category.Id
            };
            await _dbContext.Subcategories.AddAsync(subcategory);
            await _dbContext.SaveChangesAsync();

            // Act
            var exists = await _repository.ExistsForCategory(subcategory.Id, category.Id);

            // Assert
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsForCategory_ForNonExistingSubcategory_ReturnsFalse()
        {
            // Arrange
            var category = new Category { CategoryName = "Służbowy" };
            await _dbContext.Categories.AddAsync(category);
            await _dbContext.SaveChangesAsync();

            // Act
            var exists = await _repository.ExistsForCategory(999, category.Id);

            // Assert
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsForCategory_ForSubcategoryNotBelongingToCategory_ReturnsFalse()
        {
            // Arrange
            var category1 = new Category { CategoryName = "Służbowy" };
            var category2 = new Category { CategoryName = "Prywatny" };
            await _dbContext.Categories.AddRangeAsync(category1, category2);
            await _dbContext.SaveChangesAsync();

            var subcategory = new Subcategory
            {
                SubcategoryName = "Szef",
                CategoryId = category1.Id // należy do category1
            };
            await _dbContext.Subcategories.AddAsync(subcategory);
            await _dbContext.SaveChangesAsync();

            // Act - sprawdzamy czy należy do category2
            var exists = await _repository.ExistsForCategory(subcategory.Id, category2.Id);

            // Assert
            exists.Should().BeFalse();
        }

        #endregion

        #region GetByCategoryId Tests

        [Fact]
        public async Task GetByCategoryId_ReturnsAllSubcategoriesForCategory()
        {
            // Arrange
            var category1 = new Category { CategoryName = "Służbowy" };
            var category2 = new Category { CategoryName = "Prywatny" };
            await _dbContext.Categories.AddRangeAsync(category1, category2);
            await _dbContext.SaveChangesAsync();

            var subcategories = new[]
            {
                new Subcategory { SubcategoryName = "Szef", CategoryId = category1.Id },
                new Subcategory { SubcategoryName = "Klient", CategoryId = category1.Id },
                new Subcategory { SubcategoryName = "Dostawca", CategoryId = category1.Id },
                new Subcategory { SubcategoryName = "Rodzina", CategoryId = category2.Id }
            };
            await _dbContext.Subcategories.AddRangeAsync(subcategories);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByCategoryId(category1.Id);

            // Assert
            result.Should().HaveCount(3);
            result.Should().Contain(s => s.SubcategoryName == "Szef");
            result.Should().Contain(s => s.SubcategoryName == "Klient");
            result.Should().Contain(s => s.SubcategoryName == "Dostawca");
            result.Should().NotContain(s => s.SubcategoryName == "Rodzina");
        }

        [Fact]
        public async Task GetByCategoryId_WhenNoSubcategories_ReturnsEmptyList()
        {
            // Arrange
            var category = new Category { CategoryName = "Prywatny" };
            await _dbContext.Categories.AddAsync(category);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByCategoryId(category.Id);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByCategoryId_ForNonExistingCategory_ReturnsEmptyList()
        {
            // Act
            var result = await _repository.GetByCategoryId(999);

            // Assert
            result.Should().BeEmpty();
        }

        #endregion
    }
}
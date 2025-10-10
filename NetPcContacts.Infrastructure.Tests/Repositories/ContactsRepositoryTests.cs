using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NetPcContacts.Domain.Constants;
using NetPcContacts.Domain.Entities;
using NetPcContacts.Infrastructure.Persistence;
using NetPcContacts.Infrastructure.Repositories;

namespace NetPcContacts.Infrastructure.Tests.Repositories
{
    /// <summary>
    /// Testy jednostkowe dla ContactsRepository.
    /// U¿ywaj¹ InMemory Database do testowania operacji na bazie danych.
    /// </summary>
    public class ContactsRepositoryTests : IDisposable
    {
        private readonly NetPcContactsDbContext _dbContext;
        private readonly ContactsRepository _repository;

        public ContactsRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<NetPcContactsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unikalny DB dla ka¿dego testu
                .Options;

            _dbContext = new NetPcContactsDbContext(options);
            _repository = new ContactsRepository(_dbContext);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        #region Create Tests

        [Fact]
        public async Task Create_AddsContactToDatabase_ReturnsGeneratedId()
        {
            // Arrange
            var category = new Category { CategoryName = "S³u¿bowy" };
            await _dbContext.Categories.AddAsync(category);
            await _dbContext.SaveChangesAsync();

            var contact = new Contact
            {
                Name = "Jan",
                Surname = "Kowalski",
                Email = "jan.kowalski@example.com",
                PasswordHash = "hashed_password",
                PhoneNumber = "+48 123 456 789",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryId = category.Id
            };

            // Act
            var contactId = await _repository.Create(contact);

            // Assert
            contactId.Should().BeGreaterThan(0);
            var savedContact = await _dbContext.Contacts.FindAsync(contactId);
            savedContact.Should().NotBeNull();
            savedContact!.Email.Should().Be(contact.Email);
        }

        #endregion

        #region GetById Tests

        [Fact]
        public async Task GetById_ForExistingContact_ReturnsContactWithRelations()
        {
            // Arrange
            var category = new Category { CategoryName = "Prywatny" };
            await _dbContext.Categories.AddAsync(category);
            await _dbContext.SaveChangesAsync();

            var contact = new Contact
            {
                Name = "Anna",
                Surname = "Nowak",
                Email = "anna.nowak@example.com",
                PasswordHash = "hashed_password",
                PhoneNumber = "+48 987 654 321",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
                CategoryId = category.Id
            };
            await _dbContext.Contacts.AddAsync(contact);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetById(contact.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(contact.Id);
            result.Email.Should().Be(contact.Email);
            result.Category.Should().NotBeNull();
            result.Category.CategoryName.Should().Be("Prywatny");
        }

        [Fact]
        public async Task GetById_ForNonExistingContact_ReturnsNull()
        {
            // Act
            var result = await _repository.GetById(999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetByEmail Tests

        [Fact]
        public async Task GetByEmail_ForExistingEmail_ReturnsContact()
        {
            // Arrange
            var category = new Category { CategoryName = "Inny" };
            await _dbContext.Categories.AddAsync(category);
            await _dbContext.SaveChangesAsync();

            var contact = new Contact
            {
                Name = "Piotr",
                Surname = "Wiœniewski",
                Email = "piotr.wisniewski@example.com",
                PasswordHash = "hashed_password",
                PhoneNumber = "+48 111 222 333",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-35)),
                CategoryId = category.Id
            };
            await _dbContext.Contacts.AddAsync(contact);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByEmail("piotr.wisniewski@example.com");

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Piotr");
            result.Surname.Should().Be("Wiœniewski");
        }

        [Fact]
        public async Task GetByEmail_ForNonExistingEmail_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByEmail("nonexistent@example.com");

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region EmailExists Tests

        [Fact]
        public async Task EmailExists_ForExistingEmail_ReturnsTrue()
        {
            // Arrange
            var category = new Category { CategoryName = "S³u¿bowy" };
            await _dbContext.Categories.AddAsync(category);
            await _dbContext.SaveChangesAsync();

            var contact = new Contact
            {
                Name = "Maria",
                Surname = "Kowalczyk",
                Email = "maria.kowalczyk@example.com",
                PasswordHash = "hashed_password",
                PhoneNumber = "+48 444 555 666",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-28)),
                CategoryId = category.Id
            };
            await _dbContext.Contacts.AddAsync(contact);
            await _dbContext.SaveChangesAsync();

            // Act
            var exists = await _repository.EmailExists("maria.kowalczyk@example.com");

            // Assert
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task EmailExists_ForNonExistingEmail_ReturnsFalse()
        {
            // Act
            var exists = await _repository.EmailExists("nonexistent@example.com");

            // Assert
            exists.Should().BeFalse();
        }

        #endregion

        #region GetAll Tests

        [Fact]
        public async Task GetAll_ReturnsAllContactsWithRelations()
        {
            // Arrange
            var category1 = new Category { CategoryName = "S³u¿bowy" };
            var category2 = new Category { CategoryName = "Prywatny" };
            await _dbContext.Categories.AddRangeAsync(category1, category2);
            await _dbContext.SaveChangesAsync();

            var contact1 = new Contact
            {
                Name = "Jan",
                Surname = "Kowalski",
                Email = "jan@example.com",
                PasswordHash = "hash1",
                PhoneNumber = "123456789",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryId = category1.Id
            };

            var contact2 = new Contact
            {
                Name = "Anna",
                Surname = "Nowak",
                Email = "anna@example.com",
                PasswordHash = "hash2",
                PhoneNumber = "987654321",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
                CategoryId = category2.Id
            };

            await _dbContext.Contacts.AddRangeAsync(contact1, contact2);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetAll();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(c => c.Email == "jan@example.com");
            result.Should().Contain(c => c.Email == "anna@example.com");
            result.All(c => c.Category != null).Should().BeTrue();
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_RemovesContactFromDatabase()
        {
            // Arrange
            var category = new Category { CategoryName = "S³u¿bowy" };
            await _dbContext.Categories.AddAsync(category);
            await _dbContext.SaveChangesAsync();

            var contact = new Contact
            {
                Name = "Tomasz",
                Surname = "Lewandowski",
                Email = "tomasz@example.com",
                PasswordHash = "hash",
                PhoneNumber = "123456789",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-40)),
                CategoryId = category.Id
            };
            await _dbContext.Contacts.AddAsync(contact);
            await _dbContext.SaveChangesAsync();

            // Act
            await _repository.Delete(contact);

            // Assert
            var deletedContact = await _dbContext.Contacts.FindAsync(contact.Id);
            deletedContact.Should().BeNull();
        }

        #endregion

        #region SaveChanges Tests

        [Fact]
        public async Task SaveChanges_PersistsModifications()
        {
            // Arrange
            var category = new Category { CategoryName = "Prywatny" };
            await _dbContext.Categories.AddAsync(category);
            await _dbContext.SaveChangesAsync();

            var contact = new Contact
            {
                Name = "Krzysztof",
                Surname = "WoŸniak",
                Email = "krzysztof@example.com",
                PasswordHash = "hash",
                PhoneNumber = "111222333",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-33)),
                CategoryId = category.Id
            };
            await _dbContext.Contacts.AddAsync(contact);
            await _dbContext.SaveChangesAsync();

            // Modyfikacja
            contact.Name = "Krzysztof Updated";
            contact.PhoneNumber = "999888777";

            // Act
            await _repository.SaveChanges();

            // Assert
            var updatedContact = await _dbContext.Contacts.FindAsync(contact.Id);
            updatedContact.Should().NotBeNull();
            updatedContact!.Name.Should().Be("Krzysztof Updated");
            updatedContact.PhoneNumber.Should().Be("999888777");
        }

        #endregion

        #region GetAllMatchingAsync Tests

        [Fact]
        public async Task GetAllMatchingAsync_WithSearchPhrase_ReturnsMatchingContacts()
        {
            // Arrange
            var category = new Category { CategoryName = "S³u¿bowy" };
            await _dbContext.Categories.AddAsync(category);
            await _dbContext.SaveChangesAsync();

            var contacts = new[]
            {
                new Contact { Name = "Jan", Surname = "Kowalski", Email = "jan@example.com", PasswordHash = "hash", PhoneNumber = "123", BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)), CategoryId = category.Id },
                new Contact { Name = "Anna", Surname = "Nowak", Email = "anna@example.com", PasswordHash = "hash", PhoneNumber = "456", BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-25)), CategoryId = category.Id },
                new Contact { Name = "Piotr", Surname = "Kowalczyk", Email = "piotr@example.com", PasswordHash = "hash", PhoneNumber = "789", BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-35)), CategoryId = category.Id }
            };
            await _dbContext.Contacts.AddRangeAsync(contacts);
            await _dbContext.SaveChangesAsync();

            // Act
            var (result, totalCount) = await _repository.GetAllMatchingAsync("kowal", 10, 1, null, SortDirection.Ascending);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(c => c.Surname == "Kowalski");
            result.Should().Contain(c => c.Surname == "Kowalczyk");
            totalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetAllMatchingAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var category = new Category { CategoryName = "Prywatny" };
            await _dbContext.Categories.AddAsync(category);
            await _dbContext.SaveChangesAsync();

            for (int i = 1; i <= 25; i++)
            {
                await _dbContext.Contacts.AddAsync(new Contact
                {
                    Name = $"User{i}",
                    Surname = $"Surname{i}",
                    Email = $"user{i}@example.com",
                    PasswordHash = "hash",
                    PhoneNumber = $"{i}",
                    BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
                    CategoryId = category.Id
                });
            }
            await _dbContext.SaveChangesAsync();

            // Act - strona 2, rozmiar 10
            var (result, totalCount) = await _repository.GetAllMatchingAsync(null, 10, 2, null, SortDirection.Ascending);

            // Assert
            result.Should().HaveCount(10);
            totalCount.Should().Be(25);
        }

        [Fact]
        public async Task GetAllMatchingAsync_WithSorting_ReturnsSortedResults()
        {
            // Arrange
            var category = new Category { CategoryName = "S³u¿bowy" };
            await _dbContext.Categories.AddAsync(category);
            await _dbContext.SaveChangesAsync();

            var contacts = new[]
            {
                new Contact { Name = "Zenon", Surname = "Zawadzki", Email = "z@example.com", PasswordHash = "hash", PhoneNumber = "1", BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)), CategoryId = category.Id },
                new Contact { Name = "Adam", Surname = "Adamski", Email = "a@example.com", PasswordHash = "hash", PhoneNumber = "2", BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-25)), CategoryId = category.Id },
                new Contact { Name = "Marek", Surname = "Malinowski", Email = "m@example.com", PasswordHash = "hash", PhoneNumber = "3", BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-35)), CategoryId = category.Id }
            };
            await _dbContext.Contacts.AddRangeAsync(contacts);
            await _dbContext.SaveChangesAsync();

            // Act - sortowanie rosn¹co po FirstName
            var (resultAsc, _) = await _repository.GetAllMatchingAsync(null, 10, 1, "FirstName", SortDirection.Ascending);

            // Assert
            var resultList = resultAsc.ToList();
            resultList[0].Name.Should().Be("Adam");
            resultList[1].Name.Should().Be("Marek");
            resultList[2].Name.Should().Be("Zenon");
        }

        [Fact]
        public async Task GetAllMatchingAsync_WithDescendingSorting_ReturnsSortedResults()
        {
            // Arrange
            var category = new Category { CategoryName = "Inny" };
            await _dbContext.Categories.AddAsync(category);
            await _dbContext.SaveChangesAsync();

            var contacts = new[]
            {
                new Contact { Name = "Adam", Surname = "Zawadzki", Email = "az@example.com", PasswordHash = "hash", PhoneNumber = "1", BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)), CategoryId = category.Id },
                new Contact { Name = "Beata", Surname = "Nowak", Email = "bn@example.com", PasswordHash = "hash", PhoneNumber = "2", BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-25)), CategoryId = category.Id },
                new Contact { Name = "Celina", Surname = "Adamska", Email = "ca@example.com", PasswordHash = "hash", PhoneNumber = "3", BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-35)), CategoryId = category.Id }
            };
            await _dbContext.Contacts.AddRangeAsync(contacts);
            await _dbContext.SaveChangesAsync();

            // Act - sortowanie malej¹co po LastName
            var (resultDesc, _) = await _repository.GetAllMatchingAsync(null, 10, 1, "LastName", SortDirection.Descending);

            // Assert
            var resultList = resultDesc.ToList();
            resultList[0].Surname.Should().Be("Zawadzki");
            resultList[1].Surname.Should().Be("Nowak");
            resultList[2].Surname.Should().Be("Adamska");
        }

        #endregion
    }
}

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NetPcContacts.Application.Contacts.Queries.GetAllContacts;
using NetPcContacts.Domain.Constants;
using NetPcContacts.Domain.Entities;
using NetPcContacts.Domain.IRepositories;

namespace NetPcContacts.Application.Tests.Contacts.Queries.GetAllContacts
{
    /// <summary>
    /// Testy jednostkowe dla GetAllContactsQueryHandler.
    /// Sprawdzaj� poprawno�� pobierania stronicowanej listy kontakt�w.
    /// </summary>
    public class GetAllContactsQueryHandlerTests
    {
        private readonly Mock<IContactsRepository> _contactsRepositoryMock;
        private readonly Mock<ILogger<GetAllContactsQueryHandler>> _loggerMock;
        private readonly GetAllContactsQueryHandler _handler;

        public GetAllContactsQueryHandlerTests()
        {
            _contactsRepositoryMock = new Mock<IContactsRepository>();
            _loggerMock = new Mock<ILogger<GetAllContactsQueryHandler>>();
            _handler = new GetAllContactsQueryHandler(
                _contactsRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsPagedResultWithContacts()
        {
            // Arrange
            var category = new Category { Id = 1, CategoryName = "S�u�bowy" };
            var contacts = new List<Contact>
            {
                new Contact
                {
                    Id = 1,
                    Name = "Jan",
                    Surname = "Kowalski",
                    Email = "jan@example.com",
                    PasswordHash = "hash",
                    PhoneNumber = "123456789",
                    BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                    CategoryId = 1,
                    Category = category
                },
                new Contact
                {
                    Id = 2,
                    Name = "Anna",
                    Surname = "Nowak",
                    Email = "anna@example.com",
                    PasswordHash = "hash",
                    PhoneNumber = "987654321",
                    BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
                    CategoryId = 1,
                    Category = category
                }
            };

            var query = new GetAllContactsQuery
            {
                PageNumber = 1,
                PageSize = 10,
                SearchPhrase = null,
                SortBy = null,
                SortDirection = SortDirection.Ascending
            };

            _contactsRepositoryMock.Setup(r => r.GetAllMatchingAsync(
                query.SearchPhrase,
                query.PageSize,
                query.PageNumber,
                query.SortBy,
                query.SortDirection))
                .ReturnsAsync((contacts, 2));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalItemsCount.Should().Be(2);
            result.TotalPages.Should().Be(1);
            result.ItemsFrom.Should().Be(1);
            result.ItemsTo.Should().Be(10); // ItemsFrom + PageSize - 1

            var firstContact = result.Items.First();
            firstContact.Id.Should().Be(1);
            firstContact.Name.Should().Be("Jan");
            firstContact.Surname.Should().Be("Kowalski");
            firstContact.Email.Should().Be("jan@example.com");
            firstContact.Category.Should().Be("S�u�bowy");
        }

        [Fact]
        public async Task Handle_WithSearchPhrase_FiltersContacts()
        {
            // Arrange
            var category = new Category { Id = 1, CategoryName = "S�u�bowy" };
            var filteredContacts = new List<Contact>
            {
                new Contact
                {
                    Id = 1,
                    Name = "Jan",
                    Surname = "Kowalski",
                    Email = "jan.kowalski@example.com",
                    PasswordHash = "hash",
                    PhoneNumber = "123456789",
                    BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                    CategoryId = 1,
                    Category = category
                }
            };

            var query = new GetAllContactsQuery
            {
                PageNumber = 1,
                PageSize = 10,
                SearchPhrase = "kowal",
                SortBy = null,
                SortDirection = SortDirection.Ascending
            };

            _contactsRepositoryMock.Setup(r => r.GetAllMatchingAsync(
                query.SearchPhrase,
                query.PageSize,
                query.PageNumber,
                query.SortBy,
                query.SortDirection))
                .ReturnsAsync((filteredContacts, 1));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Items.Should().HaveCount(1);
            result.TotalItemsCount.Should().Be(1);
            _contactsRepositoryMock.Verify(r => r.GetAllMatchingAsync(
                "kowal",
                query.PageSize,
                query.PageNumber,
                query.SortBy,
                query.SortDirection),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var category = new Category { Id = 1, CategoryName = "Prywatny" };
            var secondPageContacts = new List<Contact>
            {
                new Contact
                {
                    Id = 11,
                    Name = "User11",
                    Surname = "Surname11",
                    Email = "user11@example.com",
                    PasswordHash = "hash",
                    PhoneNumber = "11",
                    BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
                    CategoryId = 1,
                    Category = category
                }
            };

            var query = new GetAllContactsQuery
            {
                PageNumber = 2,
                PageSize = 10,
                SearchPhrase = null,
                SortBy = null,
                SortDirection = SortDirection.Ascending
            };

            _contactsRepositoryMock.Setup(r => r.GetAllMatchingAsync(
                query.SearchPhrase,
                query.PageSize,
                query.PageNumber,
                query.SortBy,
                query.SortDirection))
                .ReturnsAsync((secondPageContacts, 15)); // 15 total items

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.TotalPages.Should().Be(2); // 15 items / 10 per page = 2 pages
            result.ItemsFrom.Should().Be(11); // strona 2, pocz�tek od elementu 11
            result.ItemsTo.Should().Be(20); // ItemsFrom + PageSize - 1 = 11 + 10 - 1 = 20
        }

        [Fact]
        public async Task Handle_WithSorting_PassesSortParametersToRepository()
        {
            // Arrange
            var category = new Category { Id = 1, CategoryName = "S�u�bowy" };
            var sortedContacts = new List<Contact>
            {
                new Contact
                {
                    Id = 1,
                    Name = "Adam",
                    Surname = "Zawadzki",
                    Email = "adam@example.com",
                    PasswordHash = "hash",
                    PhoneNumber = "123",
                    BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                    CategoryId = 1,
                    Category = category
                }
            };

            var query = new GetAllContactsQuery
            {
                PageNumber = 1,
                PageSize = 10,
                SearchPhrase = null,
                SortBy = "FirstName",
                SortDirection = SortDirection.Descending
            };

            _contactsRepositoryMock.Setup(r => r.GetAllMatchingAsync(
                query.SearchPhrase,
                query.PageSize,
                query.PageNumber,
                query.SortBy,
                query.SortDirection))
                .ReturnsAsync((sortedContacts, 1));

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _contactsRepositoryMock.Verify(r => r.GetAllMatchingAsync(
                null,
                10,
                1,
                "FirstName",
                SortDirection.Descending),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WhenNoContacts_ReturnsEmptyPagedResult()
        {
            // Arrange
            var query = new GetAllContactsQuery
            {
                PageNumber = 1,
                PageSize = 10,
                SearchPhrase = "nonexistent",
                SortBy = null,
                SortDirection = SortDirection.Ascending
            };

            _contactsRepositoryMock.Setup(r => r.GetAllMatchingAsync(
                query.SearchPhrase,
                query.PageSize,
                query.PageNumber,
                query.SortBy,
                query.SortDirection))
                .ReturnsAsync((new List<Contact>(), 0));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalItemsCount.Should().Be(0);
            result.TotalPages.Should().Be(0);
        }

        [Fact]
        public async Task Handle_MapsContactsToBasicContactDto()
        {
            // Arrange
            var category = new Category { Id = 1, CategoryName = "Inny" };
            var contacts = new List<Contact>
            {
                new Contact
                {
                    Id = 5,
                    Name = "Test",
                    Surname = "User",
                    Email = "test.user@example.com",
                    PasswordHash = "this_should_not_be_in_dto",
                    PhoneNumber = "+48 555 666 777",
                    BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-28)),
                    CategoryId = 1,
                    Category = category
                }
            };

            var query = new GetAllContactsQuery
            {
                PageNumber = 1,
                PageSize = 10,
                SortDirection = SortDirection.Ascending
            };

            _contactsRepositoryMock.Setup(r => r.GetAllMatchingAsync(
                query.SearchPhrase,
                query.PageSize,
                query.PageNumber,
                query.SortBy,
                query.SortDirection))
                .ReturnsAsync((contacts, 1));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            var dto = result.Items.First();
            dto.Id.Should().Be(5);
            dto.Name.Should().Be("Test");
            dto.Surname.Should().Be("User");
            dto.Email.Should().Be("test.user@example.com");
            dto.PhoneNumber.Should().Be("+48 555 666 777");
            dto.Category.Should().Be("Inny");

            // Sprawd�, �e PasswordHash nie jest w DTO
            var dtoProperties = dto.GetType().GetProperties();
            dtoProperties.Should().NotContain(p => p.Name == "PasswordHash" || p.Name == "Password");
        }
    }
}

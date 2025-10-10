using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NetPcContacts.Application.Contacts.Queries.GetContactById;
using NetPcContacts.Domain.Entities;
using NetPcContacts.Domain.Exceptions;
using NetPcContacts.Domain.IRepositories;

namespace NetPcContacts.Application.Tests.Contacts.Queries.GetContactById
{
    /// <summary>
    /// Testy jednostkowe dla GetContactByIdQueryHandler.
    /// Sprawdzaj¹ poprawnoœæ pobierania i mapowania danych kontaktu.
    /// </summary>
    public class GetContactByIdQueryHandlerTests
    {
        private readonly Mock<IContactsRepository> _contactsRepositoryMock;
        private readonly Mock<ILogger<GetContactByIdQueryHandler>> _loggerMock;
        private readonly GetContactByIdQueryHandler _handler;

        public GetContactByIdQueryHandlerTests()
        {
            _contactsRepositoryMock = new Mock<IContactsRepository>();
            _loggerMock = new Mock<ILogger<GetContactByIdQueryHandler>>();
            _handler = new GetContactByIdQueryHandler(
                _contactsRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ForExistingContact_ReturnsContactDto()
        {
            // Arrange
            var contactId = 1;
            var category = new Category { Id = 1, CategoryName = "S³u¿bowy" };
            var subcategory = new Subcategory { Id = 2, SubcategoryName = "Szef", CategoryId = 1 };
            
            var contact = new Contact
            {
                Id = contactId,
                Name = "Jan",
                Surname = "Kowalski",
                Email = "jan.kowalski@example.com",
                PasswordHash = "hashed_password_should_not_be_returned",
                PhoneNumber = "+48 123 456 789",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryId = 1,
                Category = category,
                SubcategoryId = 2,
                Subcategory = subcategory,
                CustomSubcategory = null
            };

            var query = new GetContactByIdQuery(contactId);

            _contactsRepositoryMock.Setup(r => r.GetById(contactId))
                .ReturnsAsync(contact);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(contactId);
            result.Name.Should().Be("Jan");
            result.Surname.Should().Be("Kowalski");
            result.Email.Should().Be("jan.kowalski@example.com");
            result.PhoneNumber.Should().Be("+48 123 456 789");
            result.BirthDate.Should().Be(contact.BirthDate);
            result.CategoryId.Should().Be(1);
            result.CategoryName.Should().Be("S³u¿bowy");
            result.SubcategoryId.Should().Be(2);
            result.SubcategoryName.Should().Be("Szef");
            result.CustomSubcategory.Should().BeNull();

            _contactsRepositoryMock.Verify(r => r.GetById(contactId), Times.Once);
        }

        [Fact]
        public async Task Handle_ForContactWithoutSubcategory_ReturnsContactDtoWithNullSubcategory()
        {
            // Arrange
            var contactId = 2;
            var category = new Category { Id = 2, CategoryName = "Prywatny" };
            
            var contact = new Contact
            {
                Id = contactId,
                Name = "Anna",
                Surname = "Nowak",
                Email = "anna.nowak@example.com",
                PasswordHash = "hash",
                PhoneNumber = "+48 987 654 321",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
                CategoryId = 2,
                Category = category,
                SubcategoryId = null,
                Subcategory = null,
                CustomSubcategory = null
            };

            var query = new GetContactByIdQuery(contactId);

            _contactsRepositoryMock.Setup(r => r.GetById(contactId))
                .ReturnsAsync(contact);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.SubcategoryId.Should().BeNull();
            result.SubcategoryName.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ForContactWithCustomSubcategory_ReturnsContactDtoWithCustomSubcategory()
        {
            // Arrange
            var contactId = 3;
            var category = new Category { Id = 3, CategoryName = "Inny" };
            
            var contact = new Contact
            {
                Id = contactId,
                Name = "Piotr",
                Surname = "Wiœniewski",
                Email = "piotr.wisniewski@example.com",
                PasswordHash = "hash",
                PhoneNumber = "+48 111 222 333",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-35)),
                CategoryId = 3,
                Category = category,
                SubcategoryId = null,
                Subcategory = null,
                CustomSubcategory = "Partner biznesowy"
            };

            var query = new GetContactByIdQuery(contactId);

            _contactsRepositoryMock.Setup(r => r.GetById(contactId))
                .ReturnsAsync(contact);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.CustomSubcategory.Should().Be("Partner biznesowy");
            result.SubcategoryId.Should().BeNull();
            result.SubcategoryName.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ForNonExistingContact_ThrowsNotFoundException()
        {
            // Arrange
            var contactId = 999;
            var query = new GetContactByIdQuery(contactId);

            _contactsRepositoryMock.Setup(r => r.GetById(contactId))
                .ReturnsAsync((Contact?)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*contact*999*");

            _contactsRepositoryMock.Verify(r => r.GetById(contactId), Times.Once);
        }

        [Fact]
        public async Task Handle_DoesNotReturnPasswordHash()
        {
            // Arrange - test bezpieczeñstwa
            var contactId = 1;
            var category = new Category { Id = 1, CategoryName = "S³u¿bowy" };
            
            var contact = new Contact
            {
                Id = contactId,
                Name = "Secure",
                Surname = "User",
                Email = "secure@example.com",
                PasswordHash = "SUPER_SECRET_HASH_SHOULD_NOT_BE_EXPOSED",
                PhoneNumber = "123456789",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryId = 1,
                Category = category
            };

            var query = new GetContactByIdQuery(contactId);

            _contactsRepositoryMock.Setup(r => r.GetById(contactId))
                .ReturnsAsync(contact);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            
            // SprawdŸ, ¿e PasswordHash nie jest zwracany w DTO
            var dtoProperties = result!.GetType().GetProperties();
            dtoProperties.Should().NotContain(p => p.Name == "PasswordHash" || p.Name == "Password");
        }
    }
}

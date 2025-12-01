using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using NetPcContacts.Application.Contacts.Commands.CreateContact;
using NetPcContacts.Domain.Entities;
using NetPcContacts.Domain.Exceptions;
using NetPcContacts.Domain.IRepositories;

namespace NetPcContacts.Application.Tests.Contacts.Commands.CreateContact
{
    /// <summary>
    /// Testy jednostkowe dla CreateContactCommandHandler.
    /// Sprawdzaj¹ poprawnoœæ logiki tworzenia kontaktu, walidacji biznesowej i obs³ugi b³êdów.
    /// </summary>
    public class CreateContactCommandHandlerTests
    {
        private readonly Mock<ILogger<CreateContactCommandHandler>> _loggerMock;
        private readonly Mock<IContactsRepository> _contactsRepositoryMock;
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<ISubcategoryRepository> _subcategoryRepositoryMock;
        private readonly Mock<IPasswordHasher<Contact>> _passwordHasherMock;
        private readonly CreateContactCommandHandler _handler;

        public CreateContactCommandHandlerTests()
        {
            _loggerMock = new Mock<ILogger<CreateContactCommandHandler>>();
            _contactsRepositoryMock = new Mock<IContactsRepository>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _subcategoryRepositoryMock = new Mock<ISubcategoryRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher<Contact>>();

            _handler = new CreateContactCommandHandler(
                _loggerMock.Object,
                _contactsRepositoryMock.Object,
                _categoryRepositoryMock.Object,
                _subcategoryRepositoryMock.Object,
                _passwordHasherMock.Object);
        }

        [Fact]
        public async Task Handle_ForValidCommand_CreatesContactAndReturnsId()
        {
            // Arrange
            var command = new CreateContactCommand
            {
                Name = "Jan",
                Surname = "Kowalski",
                Email = "jan.kowalski@example.com",
                Password = "SecureP@ss123",
                PhoneNumber = "+48 123 456 789",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryId = 1,
                SubcategoryId = null,
                CustomSubcategory = null
            };

            var expectedContactId = 1;
            var hashedPassword = "hashed_password_123";

            _contactsRepositoryMock.Setup(r => r.EmailExists(command.Email))
                .ReturnsAsync(false);
            _categoryRepositoryMock.Setup(r => r.Exists(command.CategoryId))
                .ReturnsAsync(true);
            _passwordHasherMock.Setup(h => h.HashPassword(It.IsAny<Contact>(), command.Password))
                .Returns(hashedPassword);
            _contactsRepositoryMock.Setup(r => r.Create(It.IsAny<Contact>()))
                .ReturnsAsync(expectedContactId);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(expectedContactId);
            _contactsRepositoryMock.Verify(r => r.EmailExists(command.Email), Times.Once);
            _categoryRepositoryMock.Verify(r => r.Exists(command.CategoryId), Times.Once);
            _passwordHasherMock.Verify(h => h.HashPassword(It.IsAny<Contact>(), command.Password), Times.Once);
            _contactsRepositoryMock.Verify(r => r.Create(It.Is<Contact>(c =>
                c.Name == command.Name &&
                c.Surname == command.Surname &&
                c.Email == command.Email &&
                c.PasswordHash == hashedPassword &&
                c.PhoneNumber == command.PhoneNumber &&
                c.BirthDate == command.BirthDate &&
                c.CategoryId == command.CategoryId &&
                c.SubcategoryId == command.SubcategoryId &&
                c.CustomSubcategory == command.CustomSubcategory
            )), Times.Once);
        }

        [Fact]
        public async Task Handle_ForDuplicateEmail_ThrowsDuplicateEmailException()
        {
            // Arrange
            var command = new CreateContactCommand
            {
                Name = "Jan",
                Surname = "Kowalski",
                Email = "existing@example.com",
                Password = "SecureP@ss123",
                PhoneNumber = "+48 123 456 789",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryId = 1
            };

            _contactsRepositoryMock.Setup(r => r.EmailExists(command.Email))
                .ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<DuplicateEmailException>()
                .WithMessage($"Contact with email '{command.Email}' already exists.");

            _contactsRepositoryMock.Verify(r => r.EmailExists(command.Email), Times.Once);
            _categoryRepositoryMock.Verify(r => r.Exists(It.IsAny<int>()), Times.Never);
            _contactsRepositoryMock.Verify(r => r.Create(It.IsAny<Contact>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ForNonExistentCategory_ThrowsNotFoundException()
        {
            // Arrange
            var command = new CreateContactCommand
            {
                Name = "Jan",
                Surname = "Kowalski",
                Email = "jan.kowalski@example.com",
                Password = "SecureP@ss123",
                PhoneNumber = "+48 123 456 789",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryId = 999 // nieistniej¹ca kategoria
            };

            _contactsRepositoryMock.Setup(r => r.EmailExists(command.Email))
                .ReturnsAsync(false);
            _categoryRepositoryMock.Setup(r => r.Exists(command.CategoryId))
                .ReturnsAsync(false);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*CategoryId*999*");

            _contactsRepositoryMock.Verify(r => r.EmailExists(command.Email), Times.Once);
            _categoryRepositoryMock.Verify(r => r.Exists(command.CategoryId), Times.Once);
            _contactsRepositoryMock.Verify(r => r.Create(It.IsAny<Contact>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ForValidSubcategory_CreatesContactSuccessfully()
        {
            // Arrange
            var command = new CreateContactCommand
            {
                Name = "Jan",
                Surname = "Kowalski",
                Email = "jan.kowalski@example.com",
                Password = "SecureP@ss123",
                PhoneNumber = "+48 123 456 789",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryId = 1,
                SubcategoryId = 2 // podkategoria "Szef"
            };

            var expectedContactId = 1;
            var hashedPassword = "hashed_password";

            _contactsRepositoryMock.Setup(r => r.EmailExists(command.Email))
                .ReturnsAsync(false);
            _categoryRepositoryMock.Setup(r => r.Exists(command.CategoryId))
                .ReturnsAsync(true);
            _subcategoryRepositoryMock.Setup(r => r.ExistsForCategory(command.SubcategoryId.Value, command.CategoryId))
                .ReturnsAsync(true);
            _passwordHasherMock.Setup(h => h.HashPassword(It.IsAny<Contact>(), command.Password))
                .Returns(hashedPassword);
            _contactsRepositoryMock.Setup(r => r.Create(It.IsAny<Contact>()))
                .ReturnsAsync(expectedContactId);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(expectedContactId);
            _subcategoryRepositoryMock.Verify(r => r.ExistsForCategory(command.SubcategoryId.Value, command.CategoryId), Times.Once);
        }

        [Fact]
        public async Task Handle_ForInvalidSubcategory_ThrowsNotFoundException()
        {
            // Arrange
            var command = new CreateContactCommand
            {
                Name = "Jan",
                Surname = "Kowalski",
                Email = "jan.kowalski@example.com",
                Password = "SecureP@ss123",
                PhoneNumber = "+48 123 456 789",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryId = 1,
                SubcategoryId = 999 // nieistniej¹ca podkategoria
            };

            _contactsRepositoryMock.Setup(r => r.EmailExists(command.Email))
                .ReturnsAsync(false);
            _categoryRepositoryMock.Setup(r => r.Exists(command.CategoryId))
                .ReturnsAsync(true);
            _subcategoryRepositoryMock.Setup(r => r.ExistsForCategory(command.SubcategoryId.Value, command.CategoryId))
                .ReturnsAsync(false);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*SubcategoryId*999*");

            _subcategoryRepositoryMock.Verify(r => r.ExistsForCategory(command.SubcategoryId.Value, command.CategoryId), Times.Once);
            _contactsRepositoryMock.Verify(r => r.Create(It.IsAny<Contact>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ForSubcategoryNotMatchingCategory_ThrowsNotFoundException()
        {
            // Arrange - SubcategoryId nale¿y do innej kategorii
            var command = new CreateContactCommand
            {
                Name = "Jan",
                Surname = "Kowalski",
                Email = "jan.kowalski@example.com",
                Password = "SecureP@ss123",
                PhoneNumber = "+48 123 456 789",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryId = 1, // "S³u¿bowy"
                SubcategoryId = 5 // podkategoria nale¿¹ca do innej kategorii
            };

            _contactsRepositoryMock.Setup(r => r.EmailExists(command.Email))
                .ReturnsAsync(false);
            _categoryRepositoryMock.Setup(r => r.Exists(command.CategoryId))
                .ReturnsAsync(true);
            _subcategoryRepositoryMock.Setup(r => r.ExistsForCategory(command.SubcategoryId.Value, command.CategoryId))
                .ReturnsAsync(false); // podkategoria nie nale¿y do tej kategorii

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
            _subcategoryRepositoryMock.Verify(r => r.ExistsForCategory(command.SubcategoryId.Value, command.CategoryId), Times.Once);
        }

        [Fact]
        public async Task Handle_ForCustomSubcategory_CreatesContactWithCustomSubcategory()
        {
            // Arrange
            var command = new CreateContactCommand
            {
                Name = "Jan",
                Surname = "Kowalski",
                Email = "jan.kowalski@example.com",
                Password = "SecureP@ss123",
                PhoneNumber = "+48 123 456 789",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryId = 3, // "Inny"
                SubcategoryId = null,
                CustomSubcategory = "Partner biznesowy"
            };

            var expectedContactId = 1;
            var hashedPassword = "hashed_password";

            _contactsRepositoryMock.Setup(r => r.EmailExists(command.Email))
                .ReturnsAsync(false);
            _categoryRepositoryMock.Setup(r => r.Exists(command.CategoryId))
                .ReturnsAsync(true);
            _passwordHasherMock.Setup(h => h.HashPassword(It.IsAny<Contact>(), command.Password))
                .Returns(hashedPassword);
            _contactsRepositoryMock.Setup(r => r.Create(It.IsAny<Contact>()))
                .ReturnsAsync(expectedContactId);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(expectedContactId);
            _contactsRepositoryMock.Verify(r => r.Create(It.Is<Contact>(c =>
                c.CustomSubcategory == command.CustomSubcategory &&
                c.SubcategoryId == null
            )), Times.Once);
        }

        [Fact]
        public async Task Handle_HashesPasswordBeforeStoringContact()
        {
            // Arrange
            var command = new CreateContactCommand
            {
                Name = "Jan",
                Surname = "Kowalski",
                Email = "jan.kowalski@example.com",
                Password = "PlainTextPassword123!",
                PhoneNumber = "+48 123 456 789",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryId = 1
            };

            var hashedPassword = "super_secure_hash_xyz";
            _contactsRepositoryMock.Setup(r => r.EmailExists(command.Email))
                .ReturnsAsync(false);
            _categoryRepositoryMock.Setup(r => r.Exists(command.CategoryId))
                .ReturnsAsync(true);
            _passwordHasherMock.Setup(h => h.HashPassword(It.IsAny<Contact>(), command.Password))
                .Returns(hashedPassword);
            _contactsRepositoryMock.Setup(r => r.Create(It.IsAny<Contact>()))
                .ReturnsAsync(1);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _passwordHasherMock.Verify(h => h.HashPassword(It.IsAny<Contact>(), command.Password), Times.Once);
            _contactsRepositoryMock.Verify(r => r.Create(It.Is<Contact>(c =>
                c.PasswordHash == hashedPassword
            )), Times.Once);
        }
    }
}

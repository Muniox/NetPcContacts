using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using NetPcContacts.Application.Contacts.Commands.UpdateContact;
using NetPcContacts.Domain.Entities;
using NetPcContacts.Domain.Exceptions;
using NetPcContacts.Domain.IRepositories;

namespace NetPcContacts.Application.Tests.Contacts.Commands.UpdateContact
{
    /// <summary>
    /// Testy jednostkowe dla UpdateContactCommandHandler.
    /// Sprawdzaj¹ poprawnoœæ logiki aktualizacji kontaktu i walidacji biznesowej.
    /// </summary>
    public class UpdateContactCommandHandlerTests
    {
        private readonly Mock<IContactsRepository> _contactsRepositoryMock;
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<ISubcategoryRepository> _subcategoryRepositoryMock;
        private readonly Mock<IPasswordHasher<Contact>> _passwordHasherMock;
        private readonly Mock<ILogger<UpdateContactCommandHandler>> _loggerMock;
        private readonly UpdateContactCommandHandler _handler;

        public UpdateContactCommandHandlerTests()
        {
            _contactsRepositoryMock = new Mock<IContactsRepository>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _subcategoryRepositoryMock = new Mock<ISubcategoryRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher<Contact>>();
            _loggerMock = new Mock<ILogger<UpdateContactCommandHandler>>();

            _handler = new UpdateContactCommandHandler(
                _contactsRepositoryMock.Object,
                _categoryRepositoryMock.Object,
                _subcategoryRepositoryMock.Object,
                _passwordHasherMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ForValidCommand_UpdatesContact()
        {
            // Arrange
            var existingContact = new Contact
            {
                Id = 1,
                Name = "Jan",
                Surname = "Kowalski",
                Email = "jan.kowalski@example.com",
                PasswordHash = "old_hash",
                PhoneNumber = "+48 123 456 789",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryId = 1
            };

            var command = new UpdateContactCommand
            {
                Id = 1,
                Name = "Jan Updated",
                Surname = "Kowalski Updated",
                Email = "jan.kowalski@example.com", // ten sam email
                Password = null, // nie zmienia has³a
                PhoneNumber = "+48 999 888 777",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryId = 1
            };

            _contactsRepositoryMock.Setup(r => r.GetById(command.Id))
                .ReturnsAsync(existingContact);
            _categoryRepositoryMock.Setup(r => r.Exists(command.CategoryId))
                .ReturnsAsync(true);
            _contactsRepositoryMock.Setup(r => r.SaveChanges())
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            existingContact.Name.Should().Be(command.Name);
            existingContact.Surname.Should().Be(command.Surname);
            existingContact.PhoneNumber.Should().Be(command.PhoneNumber);
            _contactsRepositoryMock.Verify(r => r.SaveChanges(), Times.Once);
        }

        [Fact]
        public async Task Handle_ForNonExistingContact_ThrowsNotFoundException()
        {
            // Arrange
            var command = new UpdateContactCommand
            {
                Id = 999,
                Name = "Test",
                Surname = "User",
                Email = "test@example.com",
                PhoneNumber = "123456789",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
                CategoryId = 1
            };

            _contactsRepositoryMock.Setup(r => r.GetById(command.Id))
                .ReturnsAsync((Contact?)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*contact*999*");
        }

        [Fact]
        public async Task Handle_WhenChangingEmailToDuplicate_ThrowsDuplicateEmailException()
        {
            // Arrange
            var existingContact = new Contact
            {
                Id = 1,
                Name = "Jan",
                Surname = "Kowalski",
                Email = "jan.kowalski@example.com",
                PasswordHash = "hash",
                PhoneNumber = "123456789",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryId = 1
            };

            var command = new UpdateContactCommand
            {
                Id = 1,
                Name = "Jan",
                Surname = "Kowalski",
                Email = "existing@example.com", // email ju¿ u¿ywany przez inny kontakt
                PhoneNumber = "123456789",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryId = 1
            };

            _contactsRepositoryMock.Setup(r => r.GetById(command.Id))
                .ReturnsAsync(existingContact);
            _contactsRepositoryMock.Setup(r => r.EmailExists(command.Email))
                .ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<DuplicateEmailException>()
                .WithMessage("*Contact*email*existing@example.com*");
        }

        [Fact]
        public async Task Handle_WhenKeepingSameEmail_DoesNotCheckEmailDuplication()
        {
            // Arrange
            var existingContact = new Contact
            {
                Id = 1,
                Name = "Jan",
                Surname = "Kowalski",
                Email = "jan.kowalski@example.com",
                PasswordHash = "hash",
                PhoneNumber = "123456789",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryId = 1
            };

            var command = new UpdateContactCommand
            {
                Id = 1,
                Name = "Jan Updated",
                Surname = "Kowalski",
                Email = "jan.kowalski@example.com", // ten sam email - nie sprawdzamy
                PhoneNumber = "987654321",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryId = 1
            };

            _contactsRepositoryMock.Setup(r => r.GetById(command.Id))
                .ReturnsAsync(existingContact);
            _categoryRepositoryMock.Setup(r => r.Exists(command.CategoryId))
                .ReturnsAsync(true);
            _contactsRepositoryMock.Setup(r => r.SaveChanges())
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _contactsRepositoryMock.Verify(r => r.EmailExists(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithNewPassword_UpdatesPasswordHash()
        {
            // Arrange
            var existingContact = new Contact
            {
                Id = 1,
                Name = "Jan",
                Surname = "Kowalski",
                Email = "jan@example.com",
                PasswordHash = "old_hash",
                PhoneNumber = "123456789",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryId = 1
            };

            var command = new UpdateContactCommand
            {
                Id = 1,
                Name = "Jan",
                Surname = "Kowalski",
                Email = "jan@example.com",
                Password = "NewSecureP@ss123",
                PhoneNumber = "123456789",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryId = 1
            };

            var newPasswordHash = "new_hashed_password";

            _contactsRepositoryMock.Setup(r => r.GetById(command.Id))
                .ReturnsAsync(existingContact);
            _categoryRepositoryMock.Setup(r => r.Exists(command.CategoryId))
                .ReturnsAsync(true);
            _passwordHasherMock.Setup(h => h.HashPassword(It.IsAny<Contact>(), command.Password))
                .Returns(newPasswordHash);
            _contactsRepositoryMock.Setup(r => r.SaveChanges())
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            existingContact.PasswordHash.Should().Be(newPasswordHash);
            _passwordHasherMock.Verify(h => h.HashPassword(It.IsAny<Contact>(), command.Password), Times.Once);
        }

        [Fact]
        public async Task Handle_WithNullPassword_DoesNotUpdatePasswordHash()
        {
            // Arrange
            var existingContact = new Contact
            {
                Id = 1,
                Name = "Jan",
                Surname = "Kowalski",
                Email = "jan@example.com",
                PasswordHash = "existing_hash",
                PhoneNumber = "123456789",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryId = 1
            };

            var command = new UpdateContactCommand
            {
                Id = 1,
                Name = "Jan Updated",
                Surname = "Kowalski",
                Email = "jan@example.com",
                Password = null, // nie zmienia has³a
                PhoneNumber = "987654321",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryId = 1
            };

            _contactsRepositoryMock.Setup(r => r.GetById(command.Id))
                .ReturnsAsync(existingContact);
            _categoryRepositoryMock.Setup(r => r.Exists(command.CategoryId))
                .ReturnsAsync(true);
            _contactsRepositoryMock.Setup(r => r.SaveChanges())
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            existingContact.PasswordHash.Should().Be("existing_hash");
            _passwordHasherMock.Verify(h => h.HashPassword(It.IsAny<Contact>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ForInvalidCategory_ThrowsNotFoundException()
        {
            // Arrange
            var existingContact = new Contact
            {
                Id = 1,
                Name = "Jan",
                Surname = "Kowalski",
                Email = "jan@example.com",
                PasswordHash = "hash",
                PhoneNumber = "123456789",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryId = 1
            };

            var command = new UpdateContactCommand
            {
                Id = 1,
                Name = "Jan",
                Surname = "Kowalski",
                Email = "jan@example.com",
                PhoneNumber = "123456789",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryId = 999 // nieistniej¹ca kategoria
            };

            _contactsRepositoryMock.Setup(r => r.GetById(command.Id))
                .ReturnsAsync(existingContact);
            _categoryRepositoryMock.Setup(r => r.Exists(command.CategoryId))
                .ReturnsAsync(false);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*CategoryId*999*");
        }

        [Fact]
        public async Task Handle_ForInvalidSubcategory_ThrowsNotFoundException()
        {
            // Arrange
            var existingContact = new Contact
            {
                Id = 1,
                Name = "Jan",
                Surname = "Kowalski",
                Email = "jan@example.com",
                PasswordHash = "hash",
                PhoneNumber = "123456789",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryId = 1
            };

            var command = new UpdateContactCommand
            {
                Id = 1,
                Name = "Jan",
                Surname = "Kowalski",
                Email = "jan@example.com",
                PhoneNumber = "123456789",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryId = 1,
                SubcategoryId = 999 // nieistniej¹ca podkategoria
            };

            _contactsRepositoryMock.Setup(r => r.GetById(command.Id))
                .ReturnsAsync(existingContact);
            _categoryRepositoryMock.Setup(r => r.Exists(command.CategoryId))
                .ReturnsAsync(true);
            _subcategoryRepositoryMock.Setup(r => r.ExistsForCategory(command.SubcategoryId.Value, command.CategoryId))
                .ReturnsAsync(false);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*SubcategoryId*999*");
        }
    }
}

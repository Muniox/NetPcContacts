using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NetPcContacts.Application.Contacts.Commands.DeleteContact;
using NetPcContacts.Domain.Entities;
using NetPcContacts.Domain.Exceptions;
using NetPcContacts.Domain.IRepositories;

namespace NetPcContacts.Application.Tests.Contacts.Commands.DeleteContact
{
    /// <summary>
    /// Testy jednostkowe dla DeleteContactCommandHandler.
    /// Sprawdzaj¹ poprawnoœæ logiki usuwania kontaktu.
    /// </summary>
    public class DeleteContactCommandHandlerTests
    {
        private readonly Mock<IContactsRepository> _contactsRepositoryMock;
        private readonly Mock<ILogger<DeleteContactCommandHandler>> _loggerMock;
        private readonly DeleteContactCommandHandler _handler;

        public DeleteContactCommandHandlerTests()
        {
            _contactsRepositoryMock = new Mock<IContactsRepository>();
            _loggerMock = new Mock<ILogger<DeleteContactCommandHandler>>();
            _handler = new DeleteContactCommandHandler(
                _contactsRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ForExistingContact_DeletesContact()
        {
            // Arrange
            var contactId = 1;
            var existingContact = new Contact
            {
                Id = contactId,
                Name = "Jan",
                Surname = "Kowalski",
                Email = "jan.kowalski@example.com",
                PasswordHash = "hash",
                PhoneNumber = "+48 123 456 789",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryId = 1
            };

            var command = new DeleteContactCommand(contactId);

            _contactsRepositoryMock.Setup(r => r.GetById(contactId))
                .ReturnsAsync(existingContact);
            _contactsRepositoryMock.Setup(r => r.Delete(existingContact))
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _contactsRepositoryMock.Verify(r => r.GetById(contactId), Times.Once);
            _contactsRepositoryMock.Verify(r => r.Delete(existingContact), Times.Once);
        }

        [Fact]
        public async Task Handle_ForNonExistingContact_ThrowsNotFoundException()
        {
            // Arrange
            var contactId = 999;
            var command = new DeleteContactCommand(contactId);

            _contactsRepositoryMock.Setup(r => r.GetById(contactId))
                .ReturnsAsync((Contact?)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*contact*999*");

            _contactsRepositoryMock.Verify(r => r.GetById(contactId), Times.Once);
            _contactsRepositoryMock.Verify(r => r.Delete(It.IsAny<Contact>()), Times.Never);
        }

        [Fact]
        public async Task Handle_CallsRepositoryDeleteWithCorrectContact()
        {
            // Arrange
            var contactId = 5;
            var existingContact = new Contact
            {
                Id = contactId,
                Name = "Anna",
                Surname = "Nowak",
                Email = "anna.nowak@example.com",
                PasswordHash = "hash",
                PhoneNumber = "+48 987 654 321",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
                CategoryId = 2
            };

            var command = new DeleteContactCommand(contactId);

            _contactsRepositoryMock.Setup(r => r.GetById(contactId))
                .ReturnsAsync(existingContact);
            _contactsRepositoryMock.Setup(r => r.Delete(It.IsAny<Contact>()))
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _contactsRepositoryMock.Verify(r => r.Delete(
                It.Is<Contact>(c => 
                    c.Id == contactId && 
                    c.Email == existingContact.Email)
            ), Times.Once);
        }
    }
}

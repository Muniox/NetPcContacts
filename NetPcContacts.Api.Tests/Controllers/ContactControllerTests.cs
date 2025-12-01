using FluentAssertions;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NetPcContacts.Api.Controllers;
using NetPcContacts.Application.Common;
using NetPcContacts.Application.Contacts.Commands.CreateContact;
using NetPcContacts.Application.Contacts.Commands.DeleteContact;
using NetPcContacts.Application.Contacts.Commands.UpdateContact;
using NetPcContacts.Application.Contacts.Dtos;
using NetPcContacts.Application.Contacts.Queries.GetAllContacts;
using NetPcContacts.Application.Contacts.Queries.GetContactById;
using NetPcContacts.Domain.Constants;

namespace NetPcContacts.Api.Tests.Controllers
{
    /// <summary>
    /// Testy jednostkowe dla ContactController.
    /// Testują poprawność obsługi żądań HTTP i komunikację z Mediator.
    /// </summary>
    public class ContactControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly ContactController _controller;

        public ContactControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new ContactController(_mediatorMock.Object);
        }

        #region CreateContact Tests

        [Fact]
        public async Task CreateContact_ForValidCommand_ReturnsCreatedAtActionResult()
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
                CategoryId = 1
            };

            var expectedContactId = 1;
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedContactId);

            // Act
            var result = await _controller.CreateContact(command);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult!.ActionName.Should().Be(nameof(ContactController.GetContact));
            createdResult.RouteValues.Should().ContainKey("id");
            createdResult.RouteValues!["id"].Should().Be(expectedContactId);

            _mediatorMock.Verify(m => m.Send(command, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateContact_SendsCommandToMediator()
        {
            // Arrange
            var command = new CreateContactCommand
            {
                Name = "Anna",
                Surname = "Nowak",
                Email = "anna.nowak@example.com",
                Password = "SecureP@ss456",
                PhoneNumber = "+48 987 654 321",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-25)),
                CategoryId = 2
            };

            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(2);

            // Act
            await _controller.CreateContact(command);

            // Assert
            _mediatorMock.Verify(m => m.Send(
                It.Is<CreateContactCommand>(c =>
                    c.Name == command.Name &&
                    c.Email == command.Email),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion

        #region UpdateContact Tests

        [Fact]
        public async Task UpdateContact_ForValidDto_ReturnsNoContent()
        {
            // Arrange
            var contactId = 5;
            var dto = new UpdateContactDto
            {
                Name = "Jan",
                Surname = "Kowalski Updated",
                Email = "jan.updated@example.com",
                Password = null,
                PhoneNumber = "+48 999 888 777",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryId = 1
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateContactCommand>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<Unit>(Unit.Value));

            // Act
            var result = await _controller.UpdateContact(contactId, dto);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task UpdateContact_MapsDtoToCommandAndSetsIdFromRoute()
        {
            // Arrange
            var contactId = 10;
            var dto = new UpdateContactDto
            {
                Name = "Test",
                Surname = "User",
                Email = "test@example.com",
                Password = "NewP@ss123",
                PhoneNumber = "123456789",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-20)),
                CategoryId = 1,
                SubcategoryId = 2,
                CustomSubcategory = null
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateContactCommand>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<Unit>(Unit.Value));

            // Act
            await _controller.UpdateContact(contactId, dto);

            // Assert
            _mediatorMock.Verify(m => m.Send(
                It.Is<UpdateContactCommand>(c =>
                    c.Id == contactId &&
                    c.Name == dto.Name &&
                    c.Surname == dto.Surname &&
                    c.Email == dto.Email &&
                    c.Password == dto.Password &&
                    c.PhoneNumber == dto.PhoneNumber &&
                    c.BirthDate == dto.BirthDate &&
                    c.CategoryId == dto.CategoryId &&
                    c.SubcategoryId == dto.SubcategoryId &&
                    c.CustomSubcategory == dto.CustomSubcategory),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateContact_WithNullPassword_SendsCommandWithNullPassword()
        {
            // Arrange
            var contactId = 15;
            var dto = new UpdateContactDto
            {
                Name = "Jan",
                Surname = "Kowalski",
                Email = "jan@example.com",
                Password = null, // nie zmienia hasła
                PhoneNumber = "123456789",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryId = 2
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateContactCommand>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<Unit>(Unit.Value));

            // Act
            await _controller.UpdateContact(contactId, dto);

            // Assert
            _mediatorMock.Verify(m => m.Send(
                It.Is<UpdateContactCommand>(c => c.Password == null),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion

        #region GetContact Tests

        [Fact]
        public async Task GetContact_ForExistingContact_ReturnsOkWithContactDto()
        {
            // Arrange
            var contactId = 1;
            var contactDto = new ContactDto
            {
                Id = contactId,
                Name = "Jan",
                Surname = "Kowalski",
                Email = "jan.kowalski@example.com",
                PhoneNumber = "+48 123 456 789",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryName = "Służbowy",
                SubcategoryName = "Szef"
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetContactByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(contactDto);

            // Act
            var result = await _controller.GetContact(contactId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(contactDto);

            _mediatorMock.Verify(m => m.Send(
                It.Is<GetContactByIdQuery>(q => q.ContactId == contactId),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion

        #region DeleteContact Tests

        [Fact]
        public async Task DeleteContact_ForExistingContact_ReturnsNoContent()
        {
            // Arrange
            var contactId = 3;
            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteContactCommand>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<Unit>(Unit.Value));

            // Act
            var result = await _controller.DeleteContact(contactId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteContact_SendsCorrectCommandToMediator()
        {
            // Arrange
            var contactId = 7;
            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteContactCommand>(), It.IsAny<CancellationToken>()))
                .Returns(new ValueTask<Unit>(Unit.Value));

            // Act
            await _controller.DeleteContact(contactId);

            // Assert
            _mediatorMock.Verify(m => m.Send(
                It.Is<DeleteContactCommand>(c => c.ContactId == contactId),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion

        #region GetAllContacts Tests

        [Fact]
        public async Task GetAllContacts_ReturnsOkWithPagedResult()
        {
            // Arrange
            var query = new GetAllContactsQuery
            {
                SearchPhrase = "test",
                PageNumber = 1,
                PageSize = 10,
                SortBy = "FirstName",
                SortDirection = SortDirection.Ascending
            };

            var pagedResult = new PagedResult<BasicContactDto>(
                new List<BasicContactDto>
                {
                    new BasicContactDto
                    {
                        Id = 1,
                        Name = "Jan",
                        Surname = "Kowalski",
                        Email = "jan@example.com",
                        PhoneNumber = "123456789",
                        Category = "Służbowy"
                    }
                },
                1, // totalCount
                10, // pageSize
                1  // pageNumber
            );

            _mediatorMock.Setup(m => m.Send(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetAllContacts(query);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(pagedResult);

            _mediatorMock.Verify(m => m.Send(query, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAllContacts_WithNullSearchPhrase_ReturnsAllContacts()
        {
            // Arrange
            var query = new GetAllContactsQuery
            {
                SearchPhrase = null,
                PageNumber = 1,
                PageSize = 5,
                SortBy = null,
                SortDirection = SortDirection.Ascending
            };

            var pagedResult = new PagedResult<BasicContactDto>(
                new List<BasicContactDto>(),
                0,
                5,
                1
            );

            _mediatorMock.Setup(m => m.Send(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetAllContacts(query);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            _mediatorMock.Verify(m => m.Send(
                It.Is<GetAllContactsQuery>(q => q.SearchPhrase == null),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(15)]
        [InlineData(30)]
        public async Task GetAllContacts_WithDifferentPageSizes_SendsCorrectQuery(int pageSize)
        {
            // Arrange
            var query = new GetAllContactsQuery
            {
                PageNumber = 1,
                PageSize = pageSize,
                SortDirection = SortDirection.Ascending
            };

            var pagedResult = new PagedResult<BasicContactDto>(
                new List<BasicContactDto>(),
                0,
                pageSize,
                1
            );

            _mediatorMock.Setup(m => m.Send(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResult);

            // Act
            await _controller.GetAllContacts(query);

            // Assert
            _mediatorMock.Verify(m => m.Send(
                It.Is<GetAllContactsQuery>(q => q.PageSize == pageSize),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Theory]
        [InlineData("FirstName", SortDirection.Ascending)]
        [InlineData("LastName", SortDirection.Descending)]
        [InlineData("Category", SortDirection.Ascending)]
        public async Task GetAllContacts_WithSorting_SendsCorrectQuery(string sortBy, SortDirection sortDirection)
        {
            // Arrange
            var query = new GetAllContactsQuery
            {
                PageNumber = 1,
                PageSize = 10,
                SortBy = sortBy,
                SortDirection = sortDirection
            };

            var pagedResult = new PagedResult<BasicContactDto>(
                new List<BasicContactDto>(),
                0,
                10,
                1
            );

            _mediatorMock.Setup(m => m.Send(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResult);

            // Act
            await _controller.GetAllContacts(query);

            // Assert
            _mediatorMock.Verify(m => m.Send(
                It.Is<GetAllContactsQuery>(q =>
                    q.SortBy == sortBy &&
                    q.SortDirection == sortDirection),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion
    }
}

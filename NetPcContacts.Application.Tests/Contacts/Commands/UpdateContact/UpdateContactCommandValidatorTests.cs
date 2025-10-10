using FluentAssertions;
using NetPcContacts.Application.Contacts.Commands.UpdateContact;

namespace NetPcContacts.Application.Tests.Contacts.Commands.UpdateContact
{
    /// <summary>
    /// Testy jednostkowe dla UpdateContactCommandValidator.
    /// Sprawdzaj¹ poprawnoœæ walidacji danych wejœciowych przy aktualizacji kontaktu.
    /// </summary>
    public class UpdateContactCommandValidatorTests
    {
        private readonly UpdateContactCommandValidator _validator;

        public UpdateContactCommandValidatorTests()
        {
            _validator = new UpdateContactCommandValidator();
        }

        #region Id Validation Tests

        [Fact]
        public void Validate_ForValidId_ReturnsSuccess()
        {
            // Arrange
            var command = CreateValidCommand();
            command.Id = 1;

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ForZeroId_ReturnsFailure()
        {
            // Arrange
            var command = CreateValidCommand();
            command.Id = 0;

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Id" && e.ErrorMessage == "ID kontaktu musi byæ wiêksze od zera.");
        }

        [Fact]
        public void Validate_ForNegativeId_ReturnsFailure()
        {
            // Arrange
            var command = CreateValidCommand();
            command.Id = -1;

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Id" && e.ErrorMessage == "ID kontaktu musi byæ wiêksze od zera.");
        }

        #endregion

        #region Password Validation Tests (Optional)

        [Fact]
        public void Validate_ForNullPassword_ReturnsSuccess()
        {
            // Arrange - has³o opcjonalne przy update
            var command = CreateValidCommand();
            command.Password = null;

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ForEmptyPassword_ReturnsSuccess()
        {
            // Arrange - puste has³o oznacza brak zmiany
            var command = CreateValidCommand();
            command.Password = string.Empty;

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("NewP@ss123")]
        [InlineData("SecurePassword1!")]
        public void Validate_ForValidNewPassword_ReturnsSuccess(string password)
        {
            // Arrange
            var command = CreateValidCommand();
            command.Password = password;

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ForPasswordTooShort_ReturnsFailure()
        {
            // Arrange
            var command = CreateValidCommand();
            command.Password = "Pass1!"; // 6 znaków

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage == "Has³o musi zawieraæ minimum 8 znaków.");
        }

        [Fact]
        public void Validate_ForPasswordWithoutUpperCase_ReturnsFailure()
        {
            // Arrange
            var command = CreateValidCommand();
            command.Password = "password1!";

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage == "Has³o musi zawieraæ przynajmniej jedn¹ wielk¹ literê.");
        }

        [Fact]
        public void Validate_ForPasswordWithoutLowerCase_ReturnsFailure()
        {
            // Arrange
            var command = CreateValidCommand();
            command.Password = "PASSWORD1!";

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage == "Has³o musi zawieraæ przynajmniej jedn¹ ma³¹ literê.");
        }

        [Fact]
        public void Validate_ForPasswordWithoutDigit_ReturnsFailure()
        {
            // Arrange
            var command = CreateValidCommand();
            command.Password = "Password!";

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage == "Has³o musi zawieraæ przynajmniej jedn¹ cyfrê.");
        }

        [Fact]
        public void Validate_ForPasswordWithoutSpecialChar_ReturnsFailure()
        {
            // Arrange
            var command = CreateValidCommand();
            command.Password = "Password1";

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage == "Has³o musi zawieraæ przynajmniej jeden znak specjalny.");
        }

        #endregion

        #region Other Fields Validation Tests

        [Fact]
        public void Validate_ForEmptyName_ReturnsFailure()
        {
            // Arrange
            var command = CreateValidCommand();
            command.Name = string.Empty;

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Name");
        }

        [Fact]
        public void Validate_ForInvalidEmail_ReturnsFailure()
        {
            // Arrange
            var command = CreateValidCommand();
            command.Email = "invalid-email";

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Email");
        }

        [Fact]
        public void Validate_ForFutureBirthDate_ReturnsFailure()
        {
            // Arrange
            var command = CreateValidCommand();
            command.BirthDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "BirthDate");
        }

        #endregion

        /// <summary>
        /// Tworzy poprawn¹ komendê UpdateContactCommand do testów.
        /// </summary>
        private static UpdateContactCommand CreateValidCommand()
        {
            return new UpdateContactCommand
            {
                Id = 1,
                Name = "Jan",
                Surname = "Kowalski",
                Email = "jan.kowalski@example.com",
                Password = null, // opcjonalne przy update
                PhoneNumber = "+48 123 456 789",
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
                CategoryId = 1,
                SubcategoryId = null,
                CustomSubcategory = null
            };
        }
    }
}

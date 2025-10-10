using FluentAssertions;
using NetPcContacts.Application.Contacts.Commands.CreateContact;

namespace NetPcContacts.Application.Tests.Contacts.Commands.CreateContact
{
    /// <summary>
    /// Testy jednostkowe dla CreateContactCommandValidator.
    /// Sprawdzaj¹ poprawnoœæ walidacji danych wejœciowych zgodnie z regu³ami biznesowymi.
    /// </summary>
    public class CreateContactCommandValidatorTests
    {
        private readonly CreateContactCommandValidator _validator;

        public CreateContactCommandValidatorTests()
        {
            _validator = new CreateContactCommandValidator();
        }

        #region Name Validation Tests

        [Fact]
        public void Validate_ForValidName_ReturnsSuccess()
        {
            // Arrange
            var command = CreateValidCommand();
            command.Name = "Jan";

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

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
            result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage == "Imiê jest wymagane.");
        }

        [Fact]
        public void Validate_ForNameTooLong_ReturnsFailure()
        {
            // Arrange
            var command = CreateValidCommand();
            command.Name = new string('a', 101); // 101 znaków

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage == "Imiê musi zawieraæ od 1 do 100 znaków.");
        }

        #endregion

        #region Surname Validation Tests

        [Fact]
        public void Validate_ForValidSurname_ReturnsSuccess()
        {
            // Arrange
            var command = CreateValidCommand();
            command.Surname = "Kowalski";

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ForEmptySurname_ReturnsFailure()
        {
            // Arrange
            var command = CreateValidCommand();
            command.Surname = string.Empty;

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Surname" && e.ErrorMessage == "Nazwisko jest wymagane.");
        }

        [Fact]
        public void Validate_ForSurnameTooLong_ReturnsFailure()
        {
            // Arrange
            var command = CreateValidCommand();
            command.Surname = new string('a', 101);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Surname" && e.ErrorMessage == "Nazwisko musi zawieraæ od 1 do 100 znaków.");
        }

        #endregion

        #region Email Validation Tests

        [Fact]
        public void Validate_ForValidEmail_ReturnsSuccess()
        {
            // Arrange
            var command = CreateValidCommand();
            command.Email = "test@example.com";

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ForEmptyEmail_ReturnsFailure()
        {
            // Arrange
            var command = CreateValidCommand();
            command.Email = string.Empty;

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage == "Email jest wymagany.");
        }

        [Fact]
        public void Validate_ForInvalidEmailFormat_ReturnsFailure()
        {
            // Arrange
            var command = CreateValidCommand();
            command.Email = "invalid-email";

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage == "Nieprawid³owy format adresu email.");
        }

        [Fact]
        public void Validate_ForEmailTooLong_ReturnsFailure()
        {
            // Arrange
            var command = CreateValidCommand();
            command.Email = new string('a', 250) + "@test.com"; // > 255 znaków

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage == "Email nie mo¿e przekraczaæ 255 znaków.");
        }

        #endregion

        #region Password Validation Tests

        [Theory]
        [InlineData("Password1!")]
        [InlineData("SecureP@ss123")]
        [InlineData("MyP@ssw0rd")]
        public void Validate_ForValidPassword_ReturnsSuccess(string password)
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
        public void Validate_ForEmptyPassword_ReturnsFailure()
        {
            // Arrange
            var command = CreateValidCommand();
            command.Password = string.Empty;

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage == "Has³o jest wymagane.");
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
        public void Validate_ForPasswordTooLong_ReturnsFailure()
        {
            // Arrange
            var command = CreateValidCommand();
            command.Password = new string('a', 101); // 101 znaków

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage == "Has³o nie mo¿e przekraczaæ 100 znaków.");
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

        #region PhoneNumber Validation Tests

        [Theory]
        [InlineData("123456789")]
        [InlineData("+48 123 456 789")]
        [InlineData("(48) 123-456-789")]
        [InlineData("+1-234-567-8900")]
        public void Validate_ForValidPhoneNumber_ReturnsSuccess(string phoneNumber)
        {
            // Arrange
            var command = CreateValidCommand();
            command.PhoneNumber = phoneNumber;

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ForEmptyPhoneNumber_ReturnsFailure()
        {
            // Arrange
            var command = CreateValidCommand();
            command.PhoneNumber = string.Empty;

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "PhoneNumber" && e.ErrorMessage == "Numer telefonu jest wymagany.");
        }

        [Fact]
        public void Validate_ForPhoneNumberWithInvalidCharacters_ReturnsFailure()
        {
            // Arrange
            var command = CreateValidCommand();
            command.PhoneNumber = "123-456-abc";

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "PhoneNumber" && e.ErrorMessage == "Numer telefonu mo¿e zawieraæ tylko cyfry, spacje, myœlniki, plus i nawiasy.");
        }

        [Fact]
        public void Validate_ForPhoneNumberTooShort_ReturnsFailure()
        {
            // Arrange
            var command = CreateValidCommand();
            command.PhoneNumber = "12345678"; // 8 znaków

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "PhoneNumber" && e.ErrorMessage == "Numer telefonu musi zawieraæ od 9 do 20 znaków.");
        }

        [Fact]
        public void Validate_ForPhoneNumberTooLong_ReturnsFailure()
        {
            // Arrange
            var command = CreateValidCommand();
            command.PhoneNumber = new string('1', 21); // 21 znaków

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "PhoneNumber" && e.ErrorMessage == "Numer telefonu musi zawieraæ od 9 do 20 znaków.");
        }

        #endregion

        #region BirthDate Validation Tests

        [Fact]
        public void Validate_ForValidBirthDate_ReturnsSuccess()
        {
            // Arrange
            var command = CreateValidCommand();
            command.BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-25));

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
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
            result.Errors.Should().Contain(e => e.PropertyName == "BirthDate" && e.ErrorMessage == "Data urodzenia musi byæ dat¹ z przesz³oœci.");
        }

        [Fact]
        public void Validate_ForBirthDateTooOld_ReturnsFailure()
        {
            // Arrange
            var command = CreateValidCommand();
            command.BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-151));

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "BirthDate" && e.ErrorMessage == "Data urodzenia nie mo¿e byæ starsza ni¿ 150 lat.");
        }

        #endregion

        #region Subcategory Validation Tests

        [Fact]
        public void Validate_ForValidSubcategoryId_ReturnsSuccess()
        {
            // Arrange
            var command = CreateValidCommand();
            command.SubcategoryId = 1;

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ForNullSubcategoryId_ReturnsSuccess()
        {
            // Arrange
            var command = CreateValidCommand();
            command.SubcategoryId = null;

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ForInvalidSubcategoryId_ReturnsFailure()
        {
            // Arrange
            var command = CreateValidCommand();
            command.SubcategoryId = 0;

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "SubcategoryId" && e.ErrorMessage == "Podkategoria musi mieæ prawid³ow¹ wartoœæ.");
        }

        #endregion

        #region CustomSubcategory Validation Tests

        [Fact]
        public void Validate_ForValidCustomSubcategory_ReturnsSuccess()
        {
            // Arrange
            var command = CreateValidCommand();
            command.CustomSubcategory = "Partner biznesowy";

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ForNullCustomSubcategory_ReturnsSuccess()
        {
            // Arrange
            var command = CreateValidCommand();
            command.CustomSubcategory = null;

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ForCustomSubcategoryTooLong_ReturnsFailure()
        {
            // Arrange
            var command = CreateValidCommand();
            command.CustomSubcategory = new string('a', 101);

            // Act
            var result = _validator.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "CustomSubcategory" && e.ErrorMessage == "Niestandardowa podkategoria nie mo¿e przekraczaæ 100 znaków.");
        }

        #endregion

        /// <summary>
        /// Tworzy poprawn¹ komendê CreateContactCommand do testów.
        /// </summary>
        private static CreateContactCommand CreateValidCommand()
        {
            return new CreateContactCommand
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
        }
    }
}

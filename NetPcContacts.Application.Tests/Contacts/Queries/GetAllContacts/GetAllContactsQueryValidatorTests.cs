using FluentAssertions;
using NetPcContacts.Application.Contacts.Queries.GetAllContacts;
using NetPcContacts.Domain.Constants;

namespace NetPcContacts.Application.Tests.Contacts.Queries.GetAllContacts
{
    /// <summary>
    /// Testy jednostkowe dla GetAllContactsQueryValidator.
    /// Sprawdzaj¹ poprawnoœæ walidacji parametrów paginacji, wyszukiwania i sortowania.
    /// </summary>
    public class GetAllContactsQueryValidatorTests
    {
        private readonly GetAllContactsQueryValidator _validator;

        public GetAllContactsQueryValidatorTests()
        {
            _validator = new GetAllContactsQueryValidator();
        }

        #region PageNumber Validation Tests

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(100)]
        public void Validate_ForValidPageNumber_ReturnsSuccess(int pageNumber)
        {
            // Arrange
            var query = CreateValidQuery();
            query.PageNumber = pageNumber;

            // Act
            var result = _validator.Validate(query);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10)]
        public void Validate_ForInvalidPageNumber_ReturnsFailure(int pageNumber)
        {
            // Arrange
            var query = CreateValidQuery();
            query.PageNumber = pageNumber;

            // Act
            var result = _validator.Validate(query);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "PageNumber");
        }

        #endregion

        #region PageSize Validation Tests

        [Theory]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(15)]
        [InlineData(30)]
        public void Validate_ForValidPageSize_ReturnsSuccess(int pageSize)
        {
            // Arrange
            var query = CreateValidQuery();
            query.PageSize = pageSize;

            // Act
            var result = _validator.Validate(query);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(7)]
        [InlineData(20)]
        [InlineData(50)]
        [InlineData(100)]
        public void Validate_ForInvalidPageSize_ReturnsFailure(int pageSize)
        {
            // Arrange
            var query = CreateValidQuery();
            query.PageSize = pageSize;

            // Act
            var result = _validator.Validate(query);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => 
                e.PropertyName == "PageSize" && 
                e.ErrorMessage == "Page size must be in [5,10,15,30]");
        }

        #endregion

        #region SortBy Validation Tests

        [Fact]
        public void Validate_ForNullSortBy_ReturnsSuccess()
        {
            // Arrange - SortBy jest opcjonalne
            var query = CreateValidQuery();
            query.SortBy = null;

            // Act
            var result = _validator.Validate(query);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("FirstName")]
        [InlineData("LastName")]
        [InlineData("Category")]
        public void Validate_ForValidSortBy_ReturnsSuccess(string sortBy)
        {
            // Arrange
            var query = CreateValidQuery();
            query.SortBy = sortBy;

            // Act
            var result = _validator.Validate(query);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("Email")]
        [InlineData("PhoneNumber")]
        [InlineData("BirthDate")]
        [InlineData("InvalidColumn")]
        public void Validate_ForInvalidSortBy_ReturnsFailure(string sortBy)
        {
            // Arrange
            var query = CreateValidQuery();
            query.SortBy = sortBy;

            // Act
            var result = _validator.Validate(query);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => 
                e.PropertyName == "SortBy" && 
                e.ErrorMessage == "Sort by is optional, or must be in [FirstName,LastName,Category]");
        }

        #endregion

        #region SortDirection Validation Tests

        [Theory]
        [InlineData(SortDirection.Ascending)]
        [InlineData(SortDirection.Descending)]
        public void Validate_ForValidSortDirection_ReturnsSuccess(SortDirection sortDirection)
        {
            // Arrange
            var query = CreateValidQuery();
            query.SortDirection = sortDirection;

            // Act
            var result = _validator.Validate(query);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        #endregion

        #region SearchPhrase Validation Tests

        [Fact]
        public void Validate_ForNullSearchPhrase_ReturnsSuccess()
        {
            // Arrange - SearchPhrase jest opcjonalne
            var query = CreateValidQuery();
            query.SearchPhrase = null;

            // Act
            var result = _validator.Validate(query);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("Jan")]
        [InlineData("Kowalski")]
        [InlineData("jan.kowalski@example.com")]
        [InlineData("")]
        public void Validate_ForAnySearchPhrase_ReturnsSuccess(string searchPhrase)
        {
            // Arrange - SearchPhrase nie ma ograniczeñ
            var query = CreateValidQuery();
            query.SearchPhrase = searchPhrase;

            // Act
            var result = _validator.Validate(query);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        #endregion

        #region Complete Query Validation Tests

        [Fact]
        public void Validate_ForCompleteValidQuery_ReturnsSuccess()
        {
            // Arrange
            var query = new GetAllContactsQuery
            {
                SearchPhrase = "test",
                PageNumber = 2,
                PageSize = 10,
                SortBy = "LastName",
                SortDirection = SortDirection.Descending
            };

            // Act
            var result = _validator.Validate(query);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ForMinimalValidQuery_ReturnsSuccess()
        {
            // Arrange - tylko wymagane pola
            var query = new GetAllContactsQuery
            {
                SearchPhrase = null,
                PageNumber = 1,
                PageSize = 5,
                SortBy = null,
                SortDirection = SortDirection.Ascending
            };

            // Act
            var result = _validator.Validate(query);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        #endregion

        /// <summary>
        /// Tworzy poprawne zapytanie GetAllContactsQuery do testów.
        /// </summary>
        private static GetAllContactsQuery CreateValidQuery()
        {
            return new GetAllContactsQuery
            {
                SearchPhrase = null,
                PageNumber = 1,
                PageSize = 10,
                SortBy = null,
                SortDirection = SortDirection.Ascending
            };
        }
    }
}

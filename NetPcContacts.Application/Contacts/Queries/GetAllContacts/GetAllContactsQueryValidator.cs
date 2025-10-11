using FluentValidation;
using NetPcContacts.Application.Contacts.Dtos;

namespace NetPcContacts.Application.Contacts.Queries.GetAllContacts
{
    /// <summary>
    /// Walidator dla zapytania GetAllContactsQuery.
    /// Zapewnia poprawność parametrów paginacji i sortowania.
    /// </summary>
    public class GetAllContactsQueryValidator : AbstractValidator<GetAllContactsQuery>
    {
        /// <summary>
        /// Dozwolone rozmiary strony dla paginacji.
        /// </summary>
        private int[] allowPageSizes = [5, 10, 15, 30];

        /// <summary>
        /// Dozwolone nazwy kolumn, po których można sortować wyniki.
        /// </summary>
        private string[] allowedSortByColumnNames = [
            nameof(BasicContactDto.Name),
            nameof(BasicContactDto.Surname),
            nameof(BasicContactDto.Category)
            ];

        /// <summary>
        /// Inicjalizuje nową instancję walidatora i definiuje reguły walidacji.
        /// </summary>
        public GetAllContactsQueryValidator()
        {
            RuleFor(r => r.PageNumber)
           .GreaterThanOrEqualTo(1);

            RuleFor(r => r.PageSize)
                .Must(value => allowPageSizes.Contains(value))
                .WithMessage($"Page size must be in [{string.Join(",", allowPageSizes)}]");

            RuleFor(r => r.SortBy)
                .Must(value => allowedSortByColumnNames.Contains(value))
                .When(q => q.SortBy != null)
                .WithMessage($"Sort by is optional, or must be in [{string.Join(",", allowedSortByColumnNames)}]");
        }
    }
}

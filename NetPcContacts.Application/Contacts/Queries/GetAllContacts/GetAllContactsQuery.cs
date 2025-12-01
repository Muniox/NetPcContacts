using Mediator;
using NetPcContacts.Application.Common;
using NetPcContacts.Application.Contacts.Dtos;
using NetPcContacts.Domain.Constants;

namespace NetPcContacts.Application.Contacts.Queries.GetAllContacts
{
    public class GetAllContactsQuery : IQuery<PagedResult<BasicContactDto>>
    {
        public string? SearchPhrase { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SortBy { get; set; }
        public SortDirection SortDirection { get; set; }
    }
}

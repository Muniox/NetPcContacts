using MediatR;

namespace NetPcContacts.Application.Contacts.Commands.CreateContact
{
    public class CreateContactCommandHandler : IRequestHandler<CreateContactCommand, int>
    {
        public Task<int> Handle(CreateContactCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

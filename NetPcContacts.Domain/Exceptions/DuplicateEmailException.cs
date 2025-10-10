namespace NetPcContacts.Domain.Exceptions
{
    public class DuplicateEmailException : Exception
    {
        public DuplicateEmailException(string email)
            : base($"User with email '{email}' already exists")
        {
            Email = email;
        }

        public string Email { get; }
    }
}

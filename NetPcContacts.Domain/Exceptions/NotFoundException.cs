namespace NetPcContacts.Domain.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string resourceType, string resourceIdentifier)
            : base($"{resourceType} with id: {resourceIdentifier} doesn't exist")
        {
            ResourceType = resourceType;
            ResourceIdentifier = resourceIdentifier;
        }

        public string ResourceType { get; }
        public string ResourceIdentifier { get; }
    }
}

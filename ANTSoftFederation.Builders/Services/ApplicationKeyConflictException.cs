namespace SorteFederation.Builders.Services;

public sealed class ApplicationKeyConflictException() : Exception(UserMessage)
{
    public const string UserMessage = "An application with this key already exists in this workspace. Choose a different key.";
}

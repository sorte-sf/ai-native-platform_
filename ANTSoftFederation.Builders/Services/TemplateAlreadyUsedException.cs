namespace SorteFederation.Builders.Services;

public sealed class TemplateAlreadyUsedException(Guid templateId, Guid existingApplicationId, string existingApplicationName)
    : Exception("This tenant already has an application created from this template.")
{
    public Guid TemplateId { get; } = templateId;
    public Guid ExistingApplicationId { get; } = existingApplicationId;
    public string ExistingApplicationName { get; } = existingApplicationName;
}

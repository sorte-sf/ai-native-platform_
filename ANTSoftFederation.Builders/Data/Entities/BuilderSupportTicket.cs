namespace SorteFederation.Builders.Data.Domain;

public sealed class BuilderSupportTicket : BuilderEntity
{
    public Guid ApplicationId { get; set; }
    public required string Subject { get; set; }
    public required string Description { get; set; }
    public string Status { get; set; } = "open";
    public string Priority { get; set; } = "normal";
    public required string OpenedBy { get; set; }
    public string? AssignedTo { get; set; }
    public string? Resolution { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public Guid? KnowledgeDocumentId { get; set; }
    public ICollection<BuilderSupportComment> Comments { get; set; } = [];
}

public sealed class BuilderSupportComment : BuilderEntity
{
    public Guid TicketId { get; set; }
    public required string AuthorId { get; set; }
    public required string Body { get; set; }
    public BuilderSupportTicket Ticket { get; set; } = null!;
}

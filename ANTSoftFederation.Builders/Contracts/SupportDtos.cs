namespace SorteFederation.Builders.Contracts;

public sealed record OpenSupportTicketRequest(Guid TenantId, Guid ApplicationId, string Subject,
    string Description, string Priority = "normal");
public sealed record AddSupportCommentRequest(Guid TenantId, string Body);
public sealed record ResolveSupportTicketRequest(Guid TenantId, string Resolution);

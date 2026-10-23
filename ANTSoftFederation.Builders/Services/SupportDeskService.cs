using System.Security.Claims;
using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Data;
using SorteFederation.Builders.Data.Domain;
using Microsoft.EntityFrameworkCore;

namespace SorteFederation.Builders.Services;

public sealed class SupportDeskService(BuilderDbContext metadata, TenantRuntimeContextFactory factory,
    KnowledgeService knowledge, KnowledgeEmbeddingService embeddings, IHttpContextAccessor accessor)
{
    private string Actor => accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? accessor.HttpContext?.User.Identity?.Name
        ?? throw new UnauthorizedAccessException("An authenticated user is required.");

    public async Task<BuilderSupportTicket> OpenAsync(OpenSupportTicketRequest request, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(request.Subject) || request.Subject.Length > 200 ||
            string.IsNullOrWhiteSpace(request.Description) || request.Description.Length > 12_000 ||
            request.Priority is not ("low" or "normal" or "high" or "urgent"))
            throw new ArgumentException("Invalid ticket subject, description or priority.");
        if (!await metadata.Applications.AnyAsync(x => x.Id == request.ApplicationId && x.TenantId == request.TenantId, token))
            throw new KeyNotFoundException("Application was not found for this tenant.");
        await using var db = await factory.CreateAsync(request.TenantId, token);
        var ticket = new BuilderSupportTicket
        {
            TenantId = request.TenantId, ApplicationId = request.ApplicationId,
            Subject = request.Subject.Trim(), Description = request.Description.Trim(),
            Priority = request.Priority, OpenedBy = Actor
        };
        db.SupportTickets.Add(ticket);
        await db.SaveChangesAsync(token);
        return ticket;
    }

    public async Task<List<BuilderSupportTicket>> ListAsync(Guid tenantId, Guid? applicationId, string? status, CancellationToken token)
    {
        await using var db = await factory.CreateAsync(tenantId, token);
        var query = db.SupportTickets.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (applicationId.HasValue) query = query.Where(x => x.ApplicationId == applicationId);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
        return await query.OrderByDescending(x => x.UpdatedAt).Take(100).ToListAsync(token);
    }

    public async Task<object?> GetAsync(Guid tenantId, Guid id, CancellationToken token)
    {
        await using var db = await factory.CreateAsync(tenantId, token);
        var ticket = await db.SupportTickets.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, token);
        if (ticket is null) return null;
        var comments = await db.SupportComments.AsNoTracking().Where(x => x.TicketId == id)
            .OrderBy(x => x.CreatedAt).Select(x => new { x.Id, x.AuthorId, x.Body, x.CreatedAt }).ToListAsync(token);
        return new { ticket.Id, ticket.TenantId, ticket.ApplicationId, ticket.Subject, ticket.Description,
            ticket.Status, ticket.Priority, ticket.OpenedBy, ticket.AssignedTo, ticket.Resolution,
            ticket.ResolvedAt, ticket.KnowledgeDocumentId, ticket.CreatedAt, ticket.UpdatedAt, Comments = comments };
    }

    public async Task<List<BuilderSupportTicket>> SearchAsync(Guid tenantId, string query, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length > 200)
            throw new ArgumentException("Search text must contain 1 to 200 characters.");
        await using var db = await factory.CreateAsync(tenantId, token);
        var pattern = $"%{query.Trim().Replace("%", "\\%").Replace("_", "\\_")}%";
        return await db.SupportTickets.AsNoTracking()
            .Where(x => EF.Functions.ILike(x.Subject, pattern, "\\") ||
                        EF.Functions.ILike(x.Description, pattern, "\\") ||
                        (x.Resolution != null && EF.Functions.ILike(x.Resolution, pattern, "\\")))
            .OrderByDescending(x => x.UpdatedAt).Take(100).ToListAsync(token);
    }

    public async Task<BuilderSupportComment> CommentAsync(Guid id, AddSupportCommentRequest request, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(request.Body) || request.Body.Length > 12_000)
            throw new ArgumentException("Comment must contain 1 to 12,000 characters.");
        await using var db = await factory.CreateAsync(request.TenantId, token);
        var ticket = await db.SupportTickets.SingleOrDefaultAsync(x => x.Id == id, token)
            ?? throw new KeyNotFoundException("Ticket was not found.");
        if (ticket.Status == "resolved")
            throw new InvalidOperationException("Resolved tickets cannot receive comments; open a follow-up ticket.");
        var comment = new BuilderSupportComment
        {
            TenantId = request.TenantId, TicketId = id, Body = request.Body.Trim(), AuthorId = Actor
        };
        db.SupportComments.Add(comment);
        ticket.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(token);
        return comment;
    }

    public async Task<object> ResolveAsync(Guid id, ResolveSupportTicketRequest request, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(request.Resolution) || request.Resolution.Length > 12_000)
            throw new ArgumentException("Resolution must contain 1 to 12,000 characters.");
        await using var db = await factory.CreateAsync(request.TenantId, token);
        var ticket = await db.SupportTickets.Include(x => x.Comments)
            .SingleOrDefaultAsync(x => x.Id == id, token) ?? throw new KeyNotFoundException("Ticket was not found.");
        if (ticket.Status == "resolved") throw new InvalidOperationException("Ticket is already resolved.");
        var content = $"Application issue: {ticket.Subject}\n{ticket.Description}\n" +
            string.Join("\n", ticket.Comments.OrderBy(x => x.CreatedAt).Select(x => $"Comment: {x.Body}")) +
            $"\nResolution: {request.Resolution.Trim()}";
        if (content.Length > 128_000) throw new ArgumentException("Ticket knowledge is too large.");
        var document = await knowledge.AddAsync(new CreateKnowledgeDocumentRequest(
            request.TenantId, $"ticket:{id:N}", ticket.Subject, content,
            $"support-ticket:{id}", null, ticket.ApplicationId), token);
        await knowledge.PublishAsync(request.TenantId, document.Id, token);
        ticket.Resolution = request.Resolution.Trim();
        ticket.Status = "resolved";
        ticket.ResolvedAt = DateTimeOffset.UtcNow;
        ticket.UpdatedAt = DateTimeOffset.UtcNow;
        ticket.KnowledgeDocumentId = document.Id;
        await db.SaveChangesAsync(token);
        var indexed = await embeddings.IndexAsync(request.TenantId, document.Id, token);
        return new { ticket.Id, ticket.Status, ticket.KnowledgeDocumentId, ChunksIndexed = indexed,
            IndexStatus = indexed == 0 ? "pending" : "indexed" };
    }
}

using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Services;
using SorteFederation.Shared.EndPoints;

namespace SorteFederation.Builders.Endpoints;

public sealed class SupportDeskEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/support/tickets").WithTags("Support Desk");
        group.MapPost("/", (OpenSupportTicketRequest request, SupportDeskService service, CancellationToken token) =>
            service.OpenAsync(request, token)).WithName("OpenBuilderSupportTicket");
        group.MapGet("/", (Guid tenantId, Guid? applicationId, string? status, SupportDeskService service,
            CancellationToken token) => service.ListAsync(tenantId, applicationId, status, token))
            .WithName("ListBuilderSupportTickets");
        group.MapGet("/search", (Guid tenantId, string query, SupportDeskService service,
            CancellationToken token) => service.SearchAsync(tenantId, query, token))
            .WithName("SearchBuilderSupportTickets");
        group.MapGet("/{id:guid}", async (Guid id, Guid tenantId, SupportDeskService service,
            CancellationToken token) =>
        {
            var ticket = await service.GetAsync(tenantId, id, token);
            return ticket is null ? Results.NotFound() : Results.Ok(ticket);
        }).WithName("GetBuilderSupportTicket");
        group.MapPost("/{id:guid}/comments", (Guid id, AddSupportCommentRequest request,
            SupportDeskService service, CancellationToken token) => service.CommentAsync(id, request, token))
            .WithName("CommentOnBuilderSupportTicket");
        group.MapPost("/{id:guid}/resolve", (Guid id, ResolveSupportTicketRequest request,
            SupportDeskService service, CancellationToken token) => service.ResolveAsync(id, request, token))
            .RequireAuthorization(policy => policy.RequireRole(BuilderRoleNames.SystemAdmin,
                BuilderRoleNames.TenantAdmin, BuilderRoleNames.Developer))
            .WithName("ResolveBuilderSupportTicket");
    }
}

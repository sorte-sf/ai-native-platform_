using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SorteFederation.Builders.Data.Entities;
using SorteFederation.Builders.Services;

namespace SorteFederation.Builders.Endpoints;

public sealed class WorkflowApprovalPoliciesEndpoints : SorteFederation.Shared.EndPoints.IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/applications/{applicationId:guid}/workflow-approval-policies")
            .RequireAuthorization().WithTags("Workflow approval policies");
        group.MapGet("", async (Guid applicationId, Guid tenantId, BuilderDbContext db, IHttpContextAccessor accessor, CancellationToken token) =>
        {
            var app = await db.Applications.SingleOrDefaultAsync(x => x.Id == applicationId && x.TenantId == tenantId, token);
            if (app is null) return Results.NotFound();
            if (!ApplicationEditingPolicy.CanEdit(accessor.HttpContext?.User ?? new(), app)) return Results.Forbid();
            var rows = await db.WorkflowApprovalPolicies.AsNoTracking()
                .Where(x => x.ApplicationId == applicationId && x.TenantId == tenantId && x.RecordState == SorteFederation.GlobalEnums.RecordState.Active)
                .OrderBy(x => x.Name).ToListAsync(token);
            return Results.Ok(rows.Select(ToResponse));
        });
        group.MapPost("", async (Guid applicationId, Guid tenantId, SaveWorkflowApprovalPolicyRequest request, BuilderDbContext db, IHttpContextAccessor accessor, CancellationToken token) =>
        {
            if (!await CanEdit(db, applicationId, tenantId, token)) return Results.NotFound();
            if (!ApplicationEditingPolicy.CanEdit(accessor.HttpContext?.User ?? new(), await db.Applications.SingleAsync(x => x.Id == applicationId && x.TenantId == tenantId, token))) return Results.Forbid();
            var validation = Validate(request);
            if (validation is not null) return Results.BadRequest(new { error = validation });
            var key = request.Key.Trim().ToLowerInvariant();
            var row = await db.WorkflowApprovalPolicies.SingleOrDefaultAsync(x => x.TenantId == tenantId && x.ApplicationId == applicationId && x.Key == key, token);
            if (row is null) { row = new BuilderWorkflowApprovalPolicy { TenantId = tenantId, ApplicationId = applicationId, Key = key, Name = request.Name.Trim() }; db.WorkflowApprovalPolicies.Add(row); }
            else { row.Name = request.Name.Trim(); row.UpdatedAt = DateTimeOffset.UtcNow; }
            row.StepsJson = JsonSerializer.Serialize(request.Steps);
            await db.SaveChangesAsync(token);
            return Results.Ok(ToResponse(row));
        });
        group.MapDelete("/{policyId:guid}", async (Guid applicationId, Guid policyId, Guid tenantId, BuilderDbContext db, IHttpContextAccessor accessor, CancellationToken token) =>
        {
            var app = await db.Applications.SingleOrDefaultAsync(x => x.Id == applicationId && x.TenantId == tenantId, token);
            if (app is null) return Results.NotFound();
            if (!ApplicationEditingPolicy.CanEdit(accessor.HttpContext?.User ?? new(), app)) return Results.Forbid();
            var row = await db.WorkflowApprovalPolicies.SingleOrDefaultAsync(x => x.Id == policyId && x.ApplicationId == applicationId && x.TenantId == tenantId, token);
            if (row is null) return Results.NotFound();
            db.WorkflowApprovalPolicies.Remove(row);
            await db.SaveChangesAsync(token);
            return Results.NoContent();
        });
    }

    private static async Task<bool> CanEdit(BuilderDbContext db, Guid applicationId, Guid tenantId, CancellationToken token) =>
        await db.Applications.AnyAsync(x => x.Id == applicationId && x.TenantId == tenantId, token);

    private static object ToResponse(BuilderWorkflowApprovalPolicy row) => new
    {
        id = row.Id, tenantId = row.TenantId, applicationId = row.ApplicationId, key = row.Key,
        name = row.Name, steps = JsonSerializer.Deserialize<List<ApprovalLevel>>(row.StepsJson) ?? [],
        updatedAt = row.UpdatedAt
    };

    private static string? Validate(SaveWorkflowApprovalPolicyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Key) || request.Key.Length > 120) return "Policy key is required and must be at most 120 characters.";
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 160) return "Policy name is required and must be at most 160 characters.";
        if (request.Steps is null || request.Steps.Count is < 1 or > 20) return "An approval policy must contain between 1 and 20 levels.";
        if (request.Steps.Any(x => string.IsNullOrWhiteSpace(x.Name) || string.IsNullOrWhiteSpace(x.AssignedTo) || x.Name.Length > 160 || x.AssignedTo.Length > 200 || !ValidAssignees(x.AssignedTo) || x.DueHours is < 1 or > 2160 || x.EscalationHours is < 1 or > 2160 || (x.EscalateTo?.Length ?? 0) > 200 || x.Mode is not ("all" or "any"))) return "Each approval level requires valid assignees and an all or any decision mode; due and escalation hours must be between 1 and 2160.";
        return null;
    }

    private static bool ValidAssignees(string value)
    {
        var assignees = value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return assignees.Length is > 0 and <= 20 && assignees.All(x => x.Length <= 200) && assignees.Distinct(StringComparer.OrdinalIgnoreCase).Count() == assignees.Length;
    }

    public sealed record SaveWorkflowApprovalPolicyRequest(string Key, string Name, List<ApprovalLevel> Steps);
    public sealed record ApprovalLevel(string Name, string AssignedTo, int? DueHours = null, int? EscalationHours = null, string? EscalateTo = null, bool PreventRequesterApproval = true, string Mode = "all");
}

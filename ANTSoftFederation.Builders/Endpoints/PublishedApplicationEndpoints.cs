namespace SorteFederation.Builders.Endpoints;

using SorteFederation.GlobalEnums;

public sealed class PublishedApplicationEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/applications/{applicationId:guid}/published", async (
            Guid applicationId,
            Guid tenantId,
            ClaimsPrincipal user,
            BuilderDbContext db,
            BuilderAuthorizationService authorization,
            CancellationToken token) =>
        {
            var app = await db.Applications.AsNoTracking().SingleOrDefaultAsync(x => x.Id == applicationId && x.TenantId == tenantId, token);
            if (app is null || app.Status != Data.Enums.DefinitionStatus.Published || app.RecordState != RecordState.Active) return Results.NotFound();

            var version = await db.ApplicationVersions.AsNoTracking().SingleOrDefaultAsync(x => x.ApplicationId == applicationId
                && x.TenantId == tenantId && x.VersionNumber == app.PublishedVersion && x.State == Data.Enums.GovernanceState.Published, token);

            if (version is null) return Results.NotFound();

            using var bundle = JsonDocument.Parse(version.SnapshotJson);
            // Check every object before returning an immutable runtime definition; never return draft metadata.
            foreach (var obj in bundle.RootElement.GetProperty("Objects").EnumerateArray())
                await authorization.EnsureObjectAccess(tenantId, obj.GetProperty("Id").GetGuid(), "read", user, token);
            return Results.Content(version.SnapshotJson, "application/json");
        }).RequireAuthorization().WithTags("Application Runtime").WithName("GetPublishedBuilderApplication");
    }
}

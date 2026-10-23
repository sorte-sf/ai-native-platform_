using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Services;

namespace SorteFederation.Builders.Endpoints;

public sealed class FormsEndpoints : SorteFederation.Shared.EndPoints.IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapFormsEndpoints(endpoints);

    public static IEndpointRouteBuilder MapFormsEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/forms", (CreateFormRequest request, BuilderMetadataService service, CancellationToken token) => service.CreateForm(request, token))
            .RequireAuthorization().WithName("CreateBuilderForm").WithTags("Forms");

        endpoints.MapGet("/objects/{objectId:guid}/form-definition", async (Guid objectId, Guid tenantId, ClaimsPrincipal user, BuilderAuthorizationService authorization, BuilderDbContext db, CancellationToken token) =>
        {
            await authorization.EnsureObjectAccess(tenantId, objectId, "read", user, token);

            var form = await db.Forms.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.ObjectId == objectId)
                .OrderByDescending(x => x.UpdatedAt)
                .FirstOrDefaultAsync(token);

            if (form is null) return Results.NotFound();

            var fields = await db.Fields.AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.ObjectId == objectId)
                .OrderBy(x => x.SortOrder)
                .Select(x => new RuntimeFormFieldResponse(x.Key, x.Label, x.Type, x.SortOrder, x.Required, x.ReadOnly, x.ConfigurationJson))
                .ToListAsync(token);

            return Results.Ok(new RuntimeFormDefinitionResponse(form.Id, form.ObjectId, form.Key, form.Name, form.LayoutJson, fields));

        }).RequireAuthorization().WithName("GetBuilderRuntimeFormDefinition").WithTags("Forms");
        return endpoints;
    }
}


public static class BlobApiExtensions
{
    public static void MapBlobApi(this WebApplication app)
    {
        var documents = app.MapGroup("/api/v1/applications/{applicationId:guid}/documents")
            .WithTags("Application documents")
            .RequireAuthorization();

        documents.MapPost("", async (Guid applicationId, Guid tenantId, IFormFile file,
            ApplicationDocumentService service, HttpContext context, CancellationToken token) =>
        {
            if (!CanAccessTenant(context, tenantId)) return Results.Forbid();
            var document = await service.SaveAsync(tenantId, applicationId, file, token);
            return Results.Created($"/api/v1/applications/{applicationId}/documents/{document.Id}?tenantId={tenantId}", document);
        }).DisableAntiforgery()
          .RequireAuthorization(policy => policy.RequireRole("SystemAdmin", "TenantAdmin", "Developer"))
          .WithName("UploadApplicationDocument");

        documents.MapGet("/{documentId:guid}", async (Guid applicationId, Guid documentId, Guid tenantId,
            ApplicationDocumentService service, HttpContext context, CancellationToken token) =>
        {
            if (!CanAccessTenant(context, tenantId)) return Results.Forbid();
            var document = await service.FindAsync(tenantId, applicationId, documentId, token);
            return document is null
                ? (IResult)Results.NotFound()
                : Results.File(document.Path, "application/pdf", enableRangeProcessing: true);
        }).WithName("GetApplicationDocument");
    }

    private static bool CanAccessTenant(HttpContext context, Guid tenantId) =>
        context.User.IsInRole("SystemAdmin") ||
        new[] {
            "tenant_id",
            "tenantId",
            "company_id",
            "companyId"
        }.Any(claimType => Guid.TryParse(context.User.FindFirst(claimType)?.Value, out var claimTenant) && claimTenant == tenantId);

}

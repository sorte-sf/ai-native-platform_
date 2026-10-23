
namespace SorteFederation.ANTApplications.EndPoints;

public sealed class PortalTenantsEndPoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group)
    {
        group.MapPost("/portal-tenants", async (
            PortalTenantRequest request,
            ANTSoftPortalDbContext masterDb,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.ContactPersonEmailAddress))
                return Results.BadRequest("Tenant name and contact email are required.");
            var code = new string(request.Name.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(code)) return Results.BadRequest("Tenant name must contain letters or digits.");
            if (await masterDb.PortalTenants.AnyAsync(x => x.Code == code, cancellationToken))
                return Results.Conflict("A tenant with this code already exists.");

            // Tenant identity lives in the master database; runtime records for every
            // tenant live together in the shared Builders database.
            await using var transaction = await masterDb.Database.BeginTransactionAsync(cancellationToken);
            var company = new Company
            {
                Name = request.Name.Trim(),
                Address = request.Address.Trim(),
                RegistrationNo = request.RegistrationNo.Trim(),
                TaxOfficeName = request.TaxOfficeName.Trim(),
                TaxNumber = request.TaxNumber.Trim(),
                ContactPersonName = request.ContactPersonName.Trim(),
                ContactPersonTelephoneNo = request.ContactPersonTelephoneNo.Trim(),
                ContactPersonEmailAddress = request.ContactPersonEmailAddress.Trim(),
                LogoUrl = request.LogoUrl?.Trim() ?? string.Empty,
                CountryId = request.CountryId,
                ThemeKey = (request.ThemeKey ?? "default").Trim().ToLowerInvariant(),
                ThemeMode = string.Equals(request.ThemeMode, "dark", StringComparison.OrdinalIgnoreCase) ? "dark" : "light"
            };
            masterDb.Companies.Add(company);
            await masterDb.SaveChangesAsync(cancellationToken);
            var tenant = new PortalTenant(company.Name, string.Empty, string.Empty, DatabaseType.PostgreSQL)
            {
                Id = company.Id,
                Code = code,
                ThemeKey = company.ThemeKey ?? "default",
                ThemeMode = company.ThemeMode ?? "light"
            };
            masterDb.PortalTenants.Add(tenant);
            await masterDb.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Results.Created($"/api/v1/portal-tenants/{tenant.Id}", new
            {
                tenant.Id, tenant.Code, tenant.Name
            });
        }).WithTags("PortalTenants").WithName("CreatePortalTenant");
    }
}

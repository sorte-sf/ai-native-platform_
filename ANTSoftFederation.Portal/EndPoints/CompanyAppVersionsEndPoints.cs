namespace SorteFederation.ANTApplications.EndPoints;

public class CompanyAppVersionsEndPoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group)
    {
        // GET: Get all company app versions with pagination
        group.MapGet("/company-app-versions", async (
            ICompanyAppVersionService companyAppVersionService,
            int page = 1,
            int pageSize = 10,
            string search = null
            ) =>
        {
            try
            {
                var (companyAppVersions, totalCount) = await companyAppVersionService.GetCompanyAppVersionsAsync(
                    page, pageSize, search);

                return Results.Ok(new { companyAppVersions, totalCount, page, pageSize });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving company app versions: {ex.Message}");
            }
        })
        .WithTags("CompanyAppVersions")
        .WithName("GetCompanyAppVersions")
        ;

        // GET: Get company app version by ID
        group.MapGet("/company-app-versions/{id:guid}", async (
            Guid id,
            ICompanyAppVersionService companyAppVersionService
            ) =>
        {
            try
            {
                var companyAppVersion = await companyAppVersionService.GetCompanyAppVersionByIdAsync(id);

                if (companyAppVersion == null)
                    return Results.NotFound($"Company app version with ID {id} not found.");

                return Results.Ok(companyAppVersion);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving company app version: {ex.Message}");
            }
        })
        .WithTags("CompanyAppVersions")
        .WithName("GetCompanyAppVersionById")
        ;

        // POST: Create new company app version
        group.MapPost("/company-app-versions", async (
            CreateCompanyAppVersionDto createDto,
            ICompanyAppVersionService companyAppVersionService
            ) =>
        {
            try
            {
                var companyAppVersionId = await companyAppVersionService.CreateCompanyAppVersionAsync(createDto);
                return Results.Created($"/company-app-versions/{companyAppVersionId}", companyAppVersionId);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error creating company app version: {ex.Message}");
            }
        })
        .WithTags("CompanyAppVersions")
        .WithName("CreateCompanyAppVersion")
        ;

        // PUT: Update company app version
        group.MapPut("/company-app-versions/{id:guid}", async (
            Guid id,
            UpdateCompanyAppVersionDto updateDto,
            ICompanyAppVersionService companyAppVersionService
            ) =>
        {
            try
            {
                var success = await companyAppVersionService.UpdateCompanyAppVersionAsync(id, updateDto);
                
                if (!success)
                    return Results.NotFound($"Company app version with ID {id} not found.");

                return Results.Ok($"Company app version {id} updated successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error updating company app version: {ex.Message}");
            }
        })
        .WithTags("CompanyAppVersions")
        .WithName("UpdateCompanyAppVersion")
        ;

        // DELETE: Delete company app version (soft delete)
        group.MapDelete("/company-app-versions/{id:guid}", async (
            Guid id,
            ICompanyAppVersionService companyAppVersionService
            ) =>
        {
            try
            {
                var success = await companyAppVersionService.DeleteCompanyAppVersionAsync(id);

                if (!success)
                    return Results.NotFound($"Company app version with ID {id} not found.");

                return Results.Ok($"Company app version {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error deleting company app version: {ex.Message}");
            }
        })
        .WithTags("CompanyAppVersions")
        .WithName("DeleteCompanyAppVersion")
        ;
    }
}
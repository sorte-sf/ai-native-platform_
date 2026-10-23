namespace SorteFederation.ANTApplications.EndPoints;

public class CompanyApplicationLicencesEndPoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group)
    { 
        group.MapGet("/company-application-licences", async (
            ICompanyApplicationLicenceService companyApplicationLicenceService,
            int page = 1,
            int pageSize = 10,
            string search = null
            ) =>
        {
            try
            {
                var (companyApplicationLicences, totalCount) = await companyApplicationLicenceService.GetCompanyApplicationLicencesAsync(
                    page, pageSize, search);

                return Results.Ok(new { companyApplicationLicences, totalCount, page, pageSize });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving company application licences: {ex.Message}");
            }
        })
        .WithTags("CompanyApplicationLicences")
        .WithName("GetCompanyApplicationLicences")
        ;
         
        group.MapGet("/company-application-licences/{id:guid}", async (
            Guid id,
            ICompanyApplicationLicenceService companyApplicationLicenceService
            ) =>
        {
            try
            {
                var companyApplicationLicence = await companyApplicationLicenceService.GetCompanyApplicationLicenceByIdAsync(id);

                if (companyApplicationLicence == null)
                    return Results.NotFound($"Company application licence with ID {id} not found.");

                return Results.Ok(companyApplicationLicence);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving company application licence: {ex.Message}");
            }
        })
        .WithTags("CompanyApplicationLicences")
        .WithName("GetCompanyApplicationLicenceById")
        ;
         
        group.MapPost("/company-application-licences", async (
            CreateCompanyApplicationLicenceDto createDto,
            ICompanyApplicationLicenceService companyApplicationLicenceService
            ) =>
        {
            try
            {
                var companyApplicationLicenceId = await companyApplicationLicenceService.CreateCompanyApplicationLicenceAsync(createDto);
                return Results.Created($"/company-application-licences/{companyApplicationLicenceId}", companyApplicationLicenceId);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error creating company application licence: {ex.Message}");
            }
        })
        .WithTags("CompanyApplicationLicences")
        .WithName("CreateCompanyApplicationLicence")
        ;
         
        group.MapPut("/company-application-licences/{id:guid}", async (
            Guid id,
            UpdateCompanyApplicationLicenceDto updateDto,
            ICompanyApplicationLicenceService companyApplicationLicenceService
            ) =>
        {
            try
            {
                var success = await companyApplicationLicenceService.UpdateCompanyApplicationLicenceAsync(id, updateDto);
                
                if (!success)
                    return Results.NotFound($"Company application licence with ID {id} not found.");

                return Results.Ok($"Company application licence {id} updated successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error updating company application licence: {ex.Message}");
            }
        })
        .WithTags("CompanyApplicationLicences")
        .WithName("UpdateCompanyApplicationLicence")
        ;
         
        group.MapDelete("/company-application-licences/{id:guid}", async (
            Guid id,
            ICompanyApplicationLicenceService companyApplicationLicenceService
            ) =>
        {
            try
            {
                var success = await companyApplicationLicenceService.DeleteCompanyApplicationLicenceAsync(id);

                if (!success)
                    return Results.NotFound($"Company application licence with ID {id} not found.");

                return Results.Ok($"Company application licence {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error deleting company application licence: {ex.Message}");
            }
        })
        .WithTags("CompanyApplicationLicences")
        .WithName("DeleteCompanyApplicationLicence")
        ;
    }
}
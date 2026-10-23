namespace SorteFederation.Portal.EndPoints;

public class CompaniesEndPoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group)
    { 
        group.MapGet("/companies", async (
            ICompanyService companyService,
            int page = 1,
            int pageSize = 20,
            string search = null
            ) =>
        {
            try
            {
                var (companies, totalCount) = await companyService.GetCompaniesAsync(page, pageSize, search);

                return Results.Ok(new { companies, totalCount, page, pageSize });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving companies: {ex.Message}");
            }
        })
        .WithTags("Companies")
        .WithName("GetCompanies")
        .RequireAuthorization();

        group.MapGet("/companies/{id:guid}", async ( Guid id, ICompanyService companyService ) =>
        {
            try
            {
                var company = await companyService.GetCompanyByIdAsync(id);

                if (company == null)
                    return Results.NotFound($"Company with ID {id} not found.");

                return Results.Ok(company);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving company: {ex.Message}");
            }
        })
        .WithTags("Companies")
        .WithName("GetCompanyById")
        .RequireAuthorization()
        ;
         
        group.MapPost("/companies", async (CreateCompanyDto createDto, ClaimsPrincipal user, ICompanyService companyService) =>
        {
            if (!user.FindAll(ClaimTypes.Role).Any(claim => RoleNames.IsSuperAdminRole(claim.Value)))
                return Results.Forbid();

            try
            {  
                var companyId = await companyService.CreateCompanyAsync(createDto);

                return Results.Created($"/companies/{companyId}", companyId);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error creating company: {ex.Message}");
            }
        })
        .WithTags("Companies")
        .WithName("CreateCompany")
        .RequireAuthorization();
         
        group.MapPut("/companies/{id:guid}", async (Guid id, UpdateCompanyDto updateDto, ICompanyService companyService) =>
        {
            try
            {
                var success = await companyService.UpdateCompanyAsync(id, updateDto);

                if (!success)
                    return Results.NotFound($"Company with ID {id} not found.");

                return Results.Ok($"Company {id} updated successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error updating company: {ex.Message}");
            }
        })
        .WithTags("Companies")
        .WithName("UpdateCompany")
        .RequireAuthorization();
         
        group.MapDelete("/companies/{id:guid}", async ( Guid id, ICompanyService companyService) =>
        {
            try
            {
                var success = await companyService.DeleteCompanyAsync(id);

                if (!success)
                    return Results.NotFound($"Company with ID {id} not found.");

                return Results.Ok($"Company {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error deleting company: {ex.Message}");
            }
        })
        .WithTags("Companies")
        .WithName("DeleteCompany")
        .RequireAuthorization();
    }
}


/*public sealed class CompanyEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group) => EntityEndpointBuilder.MapCrud(group, typeof(Company), "/companies", "Company");
}*/
 

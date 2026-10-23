namespace SorteFederation.ANTApplications.EndPoints;

public class ProjectApplicationsEndPoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group)
    {
        group.MapGet("/project-applications", async (
            IProjectApplicationService projectApplicationService,
            int page = 1,
            int pageSize = 10,
            string search = null
            ) =>
        {
            try
            {
                var (projectApplications, totalCount) = await projectApplicationService.GetProjectApplicationsAsync(
                    page, pageSize, search);

                return Results.Ok(new { projectApplications, totalCount, page, pageSize });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving project applications: {ex.Message}");
            }
        })
        .WithTags("ProjectApplications")
        .WithName("GetProjectApplications");

        group.MapGet("/project-applications/{id:guid}", async (
            Guid id,
            IProjectApplicationService projectApplicationService
            ) =>
        {
            try
            {
                var projectApplication = await projectApplicationService.GetProjectApplicationByIdAsync(id);

                if (projectApplication == null)
                    return Results.NotFound($"Project application with ID {id} not found.");

                return Results.Ok(projectApplication);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving project application: {ex.Message}");
            }
        })
        .WithTags("ProjectApplications")
        .WithName("GetProjectApplicationById");

        group.MapPost("/project-applications", async (
            CreateProjectApplicationDto createDto,
            IProjectApplicationService projectApplicationService
            ) =>
        {
            try
            {
                var projectApplicationId = await projectApplicationService.CreateProjectApplicationAsync(createDto);
                return Results.Created($"/project-applications/{projectApplicationId}", projectApplicationId);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error creating project application: {ex.Message}");
            }
        })
        .WithTags("ProjectApplications")
        .WithName("CreateProjectApplication");

        group.MapPut("/project-applications/{id:guid}", async (
            Guid id,
            UpdateProjectApplicationDto updateDto,
            IProjectApplicationService projectApplicationService
            ) =>
        {
            try
            {
                var success = await projectApplicationService.UpdateProjectApplicationAsync(id, updateDto);

                if (!success)
                    return Results.NotFound($"Project application with ID {id} not found.");

                return Results.Ok($"Project application {id} updated successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error updating project application: {ex.Message}");
            }
        })
        .WithTags("ProjectApplications")
        .WithName("UpdateProjectApplication");

        group.MapDelete("/project-applications/{id:guid}", async (
            Guid id,
            IProjectApplicationService projectApplicationService
            ) =>
        {
            try
            {
                var success = await projectApplicationService.DeleteProjectApplicationAsync(id);

                if (!success)
                    return Results.NotFound($"Project application with ID {id} not found.");

                return Results.Ok($"Project application {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error deleting project application: {ex.Message}");
            }
        })
        .WithTags("ProjectApplications")
        .WithName("DeleteProjectApplication");
    }
}
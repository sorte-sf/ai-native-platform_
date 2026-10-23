namespace SorteFederation.ANTApplications.EndPoints;

public class ApplicationModuleIssueMessagesEndPoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group)
    {
        // GET: Get all application module issue messages with pagination
        group.MapGet("/application-module-issue-messages", async (
            IApplicationModuleIssueMessageService applicationModuleIssueMessageService,
            int page = 1,
            int pageSize = 10,
            string search = null
            ) =>
        {
            try
            {
                var (applicationModuleIssueMessages, totalCount) = await applicationModuleIssueMessageService.GetApplicationModuleIssueMessagesAsync(
                    page, pageSize, search);

                return Results.Ok(new { applicationModuleIssueMessages, totalCount, page, pageSize });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving application module issue messages: {ex.Message}");
            }
        })
        .WithTags("ApplicationModuleIssueMessages")
        .WithName("GetApplicationModuleIssueMessages");

        // GET: Get application module issue message by ID
        group.MapGet("/application-module-issue-messages/{id:guid}", async (
            Guid id,
            IApplicationModuleIssueMessageService applicationModuleIssueMessageService
            ) =>
        {
            try
            {
                var applicationModuleIssueMessage = await applicationModuleIssueMessageService.GetApplicationModuleIssueMessageByIdAsync(id);

                if (applicationModuleIssueMessage == null)
                    return Results.NotFound($"Application module issue message with ID {id} not found.");

                return Results.Ok(applicationModuleIssueMessage);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving application module issue message: {ex.Message}");
            }
        })
        .WithTags("ApplicationModuleIssueMessages")
        .WithName("GetApplicationModuleIssueMessageById");

        // POST: Create new application module issue message
        group.MapPost("/application-module-issue-messages", async (
            CreateApplicationModuleIssueMessageDto createDto,
            IApplicationModuleIssueMessageService applicationModuleIssueMessageService
            ) =>
        {
            try
            {
                var applicationModuleIssueMessageId = await applicationModuleIssueMessageService.CreateApplicationModuleIssueMessageAsync(createDto);
                return Results.Created($"/application-module-issue-messages/{applicationModuleIssueMessageId}", applicationModuleIssueMessageId);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error creating application module issue message: {ex.Message}");
            }
        })
        .WithTags("ApplicationModuleIssueMessages")
        .WithName("CreateApplicationModuleIssueMessage");

        // PUT: Update application module issue message
        group.MapPut("/application-module-issue-messages/{id:guid}", async (
            Guid id,
            UpdateApplicationModuleIssueMessageDto updateDto,
            IApplicationModuleIssueMessageService applicationModuleIssueMessageService
            ) =>
        {
            try
            {
                var success = await applicationModuleIssueMessageService.UpdateApplicationModuleIssueMessageAsync(id, updateDto);

                if (!success)
                    return Results.NotFound($"Application module issue message with ID {id} not found.");

                return Results.Ok($"Application module issue message {id} updated successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error updating application module issue message: {ex.Message}");
            }
        })
        .WithTags("ApplicationModuleIssueMessages")
        .WithName("UpdateApplicationModuleIssueMessage");

        // DELETE: Delete application module issue message (soft delete)
        group.MapDelete("/application-module-issue-messages/{id:guid}", async (
            Guid id,
            IApplicationModuleIssueMessageService applicationModuleIssueMessageService
            ) =>
        {
            try
            {
                var success = await applicationModuleIssueMessageService.DeleteApplicationModuleIssueMessageAsync(id);

                if (!success)
                    return Results.NotFound($"Application module issue message with ID {id} not found.");

                return Results.Ok($"Application module issue message {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error deleting application module issue message: {ex.Message}");
            }
        })
        .WithTags("ApplicationModuleIssueMessages")
        .WithName("DeleteApplicationModuleIssueMessage");

    }
}
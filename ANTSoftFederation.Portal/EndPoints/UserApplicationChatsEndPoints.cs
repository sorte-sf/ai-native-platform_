namespace SorteFederation.ANTApplications.EndPoints;

public class UserApplicationChatsEndPoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group)
    {
        // GET: Get all user application chats with pagination
        group.MapGet("/user-application-chats", async (
            IUserApplicationChatService userApplicationChatService,
            int page = 1,
            int pageSize = 10,
            string search = null
            ) =>
        {
            try
            {
                var (userApplicationChats, totalCount) = await userApplicationChatService.GetUserApplicationChatsAsync(
                    page, pageSize, search);

                return Results.Ok(new { userApplicationChats, totalCount, page, pageSize });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving user application chats: {ex.Message}");
            }
        })
        .WithTags("UserApplicationChats")
        .WithName("GetUserApplicationChats");

        // GET: Get user application chat by ID
        group.MapGet("/user-application-chats/{id:guid}", async (
            Guid id,
            IUserApplicationChatService userApplicationChatService
            ) =>
        {
            try
            {
                var userApplicationChat = await userApplicationChatService.GetUserApplicationChatByIdAsync(id);

                if (userApplicationChat == null)
                    return Results.NotFound($"User application chat with ID {id} not found.");

                return Results.Ok(userApplicationChat);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving user application chat: {ex.Message}");
            }
        })
        .WithTags("UserApplicationChats")
        .WithName("GetUserApplicationChatById");

        // POST: Create new user application chat
        group.MapPost("/user-application-chats", async (
            CreateUserApplicationChatDto createDto,
            IUserApplicationChatService userApplicationChatService
            ) =>
        {
            try
            {
                var userApplicationChatId = await userApplicationChatService.CreateUserApplicationChatAsync(createDto);
                return Results.Created($"/user-application-chats/{userApplicationChatId}", userApplicationChatId);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error creating user application chat: {ex.Message}");
            }
        })
        .WithTags("UserApplicationChats")
        .WithName("CreateUserApplicationChat");

        // PUT: Update user application chat
        group.MapPut("/user-application-chats/{id:guid}", async (
            Guid id,
            UpdateUserApplicationChatDto updateDto,
            IUserApplicationChatService userApplicationChatService
            ) =>
        {
            try
            {
                var success = await userApplicationChatService.UpdateUserApplicationChatAsync(id, updateDto);

                if (!success)
                    return Results.NotFound($"User application chat with ID {id} not found.");

                return Results.Ok($"User application chat {id} updated successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error updating user application chat: {ex.Message}");
            }
        })
        .WithTags("UserApplicationChats")
        .WithName("UpdateUserApplicationChat");

        // DELETE: Delete user application chat (soft delete)
        group.MapDelete("/user-application-chats/{id:guid}", async (
            Guid id,
            IUserApplicationChatService userApplicationChatService
            ) =>
        {
            try
            {
                var success = await userApplicationChatService.DeleteUserApplicationChatAsync(id);

                if (!success)
                    return Results.NotFound($"User application chat with ID {id} not found.");

                return Results.Ok($"User application chat {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error deleting user application chat: {ex.Message}");
            }
        })
        .WithTags("UserApplicationChats")
        .WithName("DeleteUserApplicationChat");
    }
}
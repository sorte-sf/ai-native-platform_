
namespace ANTSoftFederation.IdentityServer.EndPoints;

public class UserAttributeEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/user-attributes")
                      .WithTags("UserAttributes");

        group.MapGet("/", async (IUserAttributeService userAttributeService, [AsParameters] PaginationParams pagination) =>
        {
            var (userAttributes, totalCount) = await userAttributeService.GetUserAttributesAsync(
                pagination.Page, 
                pagination.PageSize, 
                pagination.Search);
            
            return Results.Ok(new { Data = userAttributes, TotalCount = totalCount });
        })
        .WithName("GetUserAttributes")
        .Produces<ApiResponse<IEnumerable<UserAttributeListDto>>>();

        group.MapGet("/{userId:guid}/{name}", async (Guid userId, string name, IUserAttributeService userAttributeService) =>
        {
            var userAttribute = await userAttributeService.GetUserAttributeByIdAsync(userId, name);
            return userAttribute != null ? Results.Ok(userAttribute) : Results.NotFound();
        })
        .WithName("GetUserAttributeById")
        .Produces<UserAttributeDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateUserAttributeDto createDto, IUserAttributeService userAttributeService) =>
        {
            var result = await userAttributeService.CreateUserAttributeAsync(createDto);
            return result ? Results.Created($"/api/user-attributes/{createDto.UserId}/{createDto.Name}", new { UserId = createDto.UserId, Name = createDto.Name }) : Results.BadRequest("User attribute already exists");
        })
        .WithName("CreateUserAttribute")
        .Accepts<CreateUserAttributeDto>("application/json")
        .Produces<bool>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{userId:guid}/{name}", async (Guid userId, string name, UpdateUserAttributeDto updateDto, IUserAttributeService userAttributeService) =>
        {
            var result = await userAttributeService.UpdateUserAttributeAsync(userId, name, updateDto);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateUserAttribute")
        .Accepts<UpdateUserAttributeDto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{userId:guid}/{name}", async (Guid userId, string name, IUserAttributeService userAttributeService) =>
        {
            var result = await userAttributeService.DeleteUserAttributeAsync(userId, name);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteUserAttribute")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}
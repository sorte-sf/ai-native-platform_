 
namespace ANTSoftFederation.IdentityServer.EndPoints;

public class GroupAttributeEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/group-attributes")
                      .WithTags("GroupAttributes");

        group.MapGet("/", async (IGroupAttributeService groupAttributeService, [AsParameters] PaginationParams pagination) =>
        {
            var (groupAttributes, totalCount) = await groupAttributeService.GetGroupAttributesAsync(
                pagination.Page, 
                pagination.PageSize, 
                pagination.Search);
            
            return Results.Ok(new { Data = groupAttributes, TotalCount = totalCount });
        })
        .WithName("GetGroupAttributes")
        .Produces<ApiResponse<IEnumerable<GroupAttributeListDto>>>();

        group.MapGet("/{id:guid}", async (Guid id, IGroupAttributeService groupAttributeService) =>
        {
            var groupAttribute = await groupAttributeService.GetGroupAttributeByIdAsync(id);
            return groupAttribute != null ? Results.Ok(groupAttribute) : Results.NotFound();
        })
        .WithName("GetGroupAttributeById")
        .Produces<GroupAttributeDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateGroupAttributeDto createDto, IGroupAttributeService groupAttributeService) =>
        {
            var id = await groupAttributeService.CreateGroupAttributeAsync(createDto);
            return Results.Created($"/api/group-attributes/{id}", new { Id = id });
        })
        .WithName("CreateGroupAttribute")
        .Accepts<CreateGroupAttributeDto>("application/json")
        .Produces<Guid>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (Guid id, UpdateGroupAttributeDto updateDto, IGroupAttributeService groupAttributeService) =>
        {
            var result = await groupAttributeService.UpdateGroupAttributeAsync(id, updateDto);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateGroupAttribute")
        .Accepts<UpdateGroupAttributeDto>("application/json")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, IGroupAttributeService groupAttributeService) =>
        {
            var result = await groupAttributeService.DeleteGroupAttributeAsync(id);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteGroupAttribute")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}
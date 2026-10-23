using Microsoft.AspNetCore.Authorization;

namespace SorteFederation.Builders.Endpoints;

public static class PublishedApplicationAiEndpoints
{
    private static readonly AuthorizationPolicy RuntimeUserPolicy = new AuthorizationPolicyBuilder(
        "BuilderAppUser", JwtBearerDefaults.AuthenticationScheme, "BuilderWorkspace")
        .RequireAuthenticatedUser().Build();

    public static void Map(IEndpointRouteBuilder endpoints)
    {
        var runtime = endpoints.MapGroup("/published-runtime").RequireAuthorization(RuntimeUserPolicy)
            .WithTags("Published application AI");

        runtime.MapPost("/assistant", async (PublishedUiRequest request, ClaimsPrincipal user,
            PublishedApplicationAiService service, CancellationToken token) =>
        {
            if (request.ApplicationId == Guid.Empty) return Results.BadRequest("Application is required.");
            return Results.Ok(await service.Generate(request.ApplicationId, request.Prompt, user, token));
        }).WithName("GeneratePublishedApplicationUi");

        runtime.MapGet("/favourites", async (Guid applicationId, ClaimsPrincipal user,
            PublishedApplicationAiService service, CancellationToken token) =>
            Results.Ok(await service.ListFavourites(applicationId, user, token)))
            .WithName("ListPublishedApplicationFavourites");

        runtime.MapPost("/favourites", async (SaveUserFavouriteRequest request, ClaimsPrincipal user,
            PublishedApplicationAiService service, CancellationToken token) =>
            Results.Ok(await service.SaveFavourite(null, request, user, token)))
            .WithName("CreatePublishedApplicationFavourite");

        runtime.MapPut("/favourites/{favouriteId:guid}", async (Guid favouriteId, SaveUserFavouriteRequest request,
            ClaimsPrincipal user, PublishedApplicationAiService service, CancellationToken token) =>
            Results.Ok(await service.SaveFavourite(favouriteId, request, user, token)))
            .WithName("UpdatePublishedApplicationFavourite");

        runtime.MapGet("/favourites/{favouriteId:guid}", async (Guid favouriteId, Guid applicationId,
            ClaimsPrincipal user, PublishedApplicationAiService service, CancellationToken token) =>
            Results.Ok(await service.OpenFavourite(applicationId, favouriteId, user, token)))
            .WithName("OpenPublishedApplicationFavourite");

        runtime.MapDelete("/favourites/{favouriteId:guid}", async (Guid favouriteId, Guid applicationId,
            ClaimsPrincipal user, PublishedApplicationAiService service, CancellationToken token) =>
            await service.DeleteFavourite(applicationId, favouriteId, user, token) ? Results.NoContent() : Results.NotFound())
            .WithName("DeletePublishedApplicationFavourite");
    }
}

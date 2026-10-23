using System.Security.Claims;
using SorteFederation.Builders.Contracts;

namespace SorteFederation.Builders.Endpoints;

public static class BuilderWorkspaceEndpoints
{
    public static void Map(IEndpointRouteBuilder endpoints)
    {
        var workspace = endpoints.MapGroup("/builder-workspace").WithTags("Builder workspace");
        workspace.MapPost("/login", async (BuilderWorkspaceLoginRequest request, BuilderWorkspaceAuthService service, CancellationToken ct) =>
        {
            var result = await service.Login(request, ct);
            return result is null ? (IResult)Results.Unauthorized() : Results.Ok(result);
        }).AllowAnonymous().WithName("BuilderWorkspaceLogin");

        workspace.MapPost("/refresh", async (ClaimsPrincipal user, BuilderWorkspaceAuthService service, CancellationToken ct) =>
        {
            var result = await service.Refresh(user, ct);
            return result is null ? (IResult)Results.Unauthorized() : Results.Ok(result);
        }).RequireAuthorization(new Microsoft.AspNetCore.Authorization.AuthorizeAttribute { AuthenticationSchemes = "BuilderWorkspace" })
          .WithName("RefreshBuilderWorkspaceToken");

        workspace.MapPost("/change-password", async (ChangeBuilderAppUserPasswordRequest request, ClaimsPrincipal user, BuilderWorkspaceAuthService service, CancellationToken ct) =>
        {
            try { return await service.ChangePassword(user, request, ct) ? (IResult)Results.NoContent() : Results.Unauthorized(); }
            catch (ArgumentException ex) { return Results.BadRequest(new { message = ex.Message }); }
        }).RequireAuthorization(new Microsoft.AspNetCore.Authorization.AuthorizeAttribute { AuthenticationSchemes = "BuilderWorkspace" })
          .WithName("ChangeBuilderWorkspacePassword");
    }
}


namespace SorteFederation.Portal.EndPoints.Addons;

public static class AddonManifestEndpoints
{
    public static IEndpointRouteBuilder MapAddonManifestEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/addons/manifests", (IConfiguration configuration) =>
        {
            var manifests = configuration.GetSection("Addons:Manifests").Get<List<AddonManifestResponse>>() ?? [];
            return Results.Ok(manifests);
        }).RequireAuthorization();

        return app;
    }

    public sealed record AddonManifestResponse(
        string Name,
        string Tag,
        string ScriptUrl,
        string ApiVersion,
        MenuContributionResponse[]? Menu,
        Dictionary<string, object>? Config,
        string? LoadMode);

    public sealed record MenuContributionResponse(
        string Id,
        string Label,
        string? Icon,
        string Route,
        string? ParentMenuId,
        int? Order,
        string? RequiredPermission);
}

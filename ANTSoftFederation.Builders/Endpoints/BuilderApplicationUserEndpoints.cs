
namespace SorteFederation.Builders.Endpoints;

public static class BuilderApplicationUserEndpoints
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/application-users/login", async (BuilderAppUserLoginRequest request, BuilderApplicationUserService service, CancellationToken token) =>
        {
            if (request is null || request.TenantId == Guid.Empty || request.ApplicationId == Guid.Empty || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrEmpty(request.Password))
                return (IResult)Results.BadRequest(new { message = "Tenant, application, email, and password are required." });
            var result = await service.Login(request, token);
            return result is null ? (IResult)Results.Unauthorized() : Results.Ok(result);
        }).AllowAnonymous().WithTags("Application users").WithName("BuilderApplicationUserLogin");

        var tenantAdmin = endpoints.MapGroup("/application-users").WithTags("Application users")
            .RequireAuthorization(JwtBearerDefaults.AuthenticationScheme)
            .AddEndpointFilter(async (EndpointFilterInvocationContext context, EndpointFilterDelegate next) =>
            {
                if (!IsTenantAdmin(context.HttpContext.User)) return Results.Forbid();
                return await next(context);
            });

        tenantAdmin.MapGet("/users", async (ClaimsPrincipal principal, BuilderApplicationUserService service, CancellationToken token) =>
        {
            var tenantId = TenantId(principal);
            return tenantId == Guid.Empty ? (IResult)Results.Forbid() : Results.Ok(await service.Users(tenantId, token));
        }).WithName("ListBuilderApplicationUsers");

        tenantAdmin.MapPost("/users", async (CreateTenantBuilderUserRequest request, ClaimsPrincipal principal, BuilderApplicationUserService service, CancellationToken token) =>
        {
            var tenantId = TenantId(principal);
            if (tenantId == Guid.Empty) return (IResult)Results.Forbid();
            try { return Results.Ok(await service.CreateTenantUser(tenantId, request, token)); }
            catch (ArgumentException ex) { return (IResult)Results.BadRequest(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Results.Conflict(new { message = ex.Message }); }
        }).WithName("CreateTenantBuilderUser");

        tenantAdmin.MapGet("/applications", async (ClaimsPrincipal principal, BuilderApplicationUserService service, CancellationToken token) =>
        {
            var tenantId = TenantId(principal);
            return tenantId == Guid.Empty ? (IResult)Results.Forbid() : Results.Ok(await service.PublishedApplications(tenantId, token));
        }).WithName("ListBuilderUserApplications");

        tenantAdmin.MapGet("/applications/{applicationId:guid}/members", async (Guid applicationId, ClaimsPrincipal principal, BuilderApplicationUserService service, CancellationToken token) =>
        {
            var tenantId = TenantId(principal);
            return tenantId == Guid.Empty ? (IResult)Results.Forbid() : Results.Ok(await service.ApplicationMembers(tenantId, applicationId, token));
        }).WithName("ListBuilderApplicationMembers");

        tenantAdmin.MapPost("/applications/{applicationId:guid}/members", async (Guid applicationId, AssignBuilderAppMembershipRequest request, ClaimsPrincipal principal, BuilderApplicationUserService service, CancellationToken token) =>
        {
            var tenantId = TenantId(principal);
            if (tenantId == Guid.Empty) return (IResult)Results.Forbid();
            try { return Results.Ok(await service.AssignMembership(tenantId, applicationId, request, token)); }
            catch (ArgumentException ex) { return (IResult)Results.BadRequest(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return (IResult)Results.NotFound(new { message = ex.Message }); }
        }).WithName("AssignBuilderApplicationMembership");

        tenantAdmin.MapDelete("/applications/{applicationId:guid}/members/{userId:guid}", async (Guid applicationId, Guid userId, ClaimsPrincipal principal, BuilderApplicationUserService service, CancellationToken token) =>
        {
            var tenantId = TenantId(principal);
            if (tenantId == Guid.Empty) return (IResult)Results.Forbid();
            return await service.RemoveMembership(tenantId, applicationId, userId, token)
                ? (IResult)Results.NoContent() : Results.NotFound();
        }).WithName("RemoveBuilderApplicationMembership");

        endpoints.MapPost("/application-users", async (CreateBuilderAppUserRequest request, ClaimsPrincipal principal, BuilderApplicationUserService service, CancellationToken token) =>
        {
            if (request is null || !IsTenantAdmin(principal)) return (IResult)Results.Forbid();
            var tenant = TenantId(principal);
            if (tenant == Guid.Empty || tenant != request.TenantId) return (IResult)Results.Forbid();
            try { return Results.Ok(await service.CreateOrAddMembership(request, token)); }
            catch (ArgumentException ex) { return (IResult)Results.BadRequest(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return (IResult)Results.NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Results.Conflict(new { message = ex.Message }); }
        }).RequireAuthorization(JwtBearerDefaults.AuthenticationScheme).WithTags("Application users").WithName("CreateBuilderApplicationUser");

        endpoints.MapPost("/application-users/change-password", async (ChangeBuilderAppUserPasswordRequest request, ClaimsPrincipal principal, BuilderApplicationUserService service, CancellationToken token) =>
        {
            if (!Guid.TryParse(principal.FindFirstValue("tenant_id"), out var tenantId)) return Results.Unauthorized();
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");
            try { return await service.ChangePassword(tenantId, userId ?? string.Empty, request, token) ? (IResult)Results.NoContent() : Results.Unauthorized(); }
            catch (ArgumentException ex) { return (IResult)Results.BadRequest(new { message = ex.Message }); }
        }).RequireAuthorization("BuilderAppUser").WithTags("Application users").WithName("ChangeBuilderApplicationUserPassword");
    }

    private static bool IsTenantAdmin(ClaimsPrincipal user) => user.IsInRole(BuilderRoleNames.TenantAdmin);

    private static Guid TenantId(ClaimsPrincipal user) => Guid.TryParse(
        user.FindFirstValue("tenant_id") ?? user.FindFirstValue("tenantId") ?? user.FindFirstValue("company_id") ?? user.FindFirstValue("companyId"), out var tenantId)
        ? tenantId : Guid.Empty;
}

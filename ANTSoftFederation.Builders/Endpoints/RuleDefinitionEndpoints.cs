using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Services;
using System.Security.Claims;

namespace SorteFederation.Builders.Endpoints;

public sealed class RuleDefinitionEndpoints : SorteFederation.Shared.EndPoints.IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints) => MapRuleDefinitionEndpoints(endpoints);

public static IEndpointRouteBuilder MapRuleDefinitionEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/rules", (CreateRuleRequest request, ClaimsPrincipal user, RuleDefinitionService service, CancellationToken token) => service.CreateAsync(request, user, token)).WithTags("Rules");
        endpoints.MapGet("/applications/{applicationId:guid}/rules", (Guid applicationId, Guid tenantId, RuleDefinitionService service, CancellationToken token) => service.ListAsync(tenantId, applicationId, token)).WithTags("Rules");
        endpoints.MapPut("/rules/{ruleId:guid}", (Guid ruleId, UpdateRuleRequest request, ClaimsPrincipal user, RuleDefinitionService service, CancellationToken token) => service.UpdateAsync(request.TenantId, ruleId, request, user, token)).WithTags("Rules");
        endpoints.MapDelete("/rules/{ruleId:guid}", async (Guid ruleId, Guid tenantId, ClaimsPrincipal user, RuleDefinitionService service, CancellationToken token) => await service.DeleteAsync(tenantId, ruleId, user, token) ? Results.NoContent() : Results.NotFound()).WithTags("Rules");
        endpoints.MapPost("/rules/{ruleId:guid}/evaluate", (Guid ruleId, Guid tenantId, string dataJson, ClaimsPrincipal user, RuleDefinitionService service, CancellationToken token) => service.EvaluateAsync(tenantId, ruleId, dataJson, user, token)).WithTags("Rules");
        endpoints.MapPost("/rules/{ruleId:guid}/execute", (Guid ruleId, ExecuteRuleRequest request, ClaimsPrincipal user, RuleDefinitionService service, CancellationToken token) => service.ExecuteAsync(request with { RuleId = ruleId }, user, token)).WithTags("Rules");
        return endpoints;
    }
}

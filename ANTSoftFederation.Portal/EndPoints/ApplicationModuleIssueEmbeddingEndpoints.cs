namespace SorteFederation.Portal.EndPoints;

public sealed class ApplicationModuleIssueEmbeddingEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group) => EntityEndpointBuilder.MapCrud(group, typeof(ApplicationModuleIssueEmbedding), "/application-module-issue-embeddings", "ApplicationModuleIssueEmbedding");
}


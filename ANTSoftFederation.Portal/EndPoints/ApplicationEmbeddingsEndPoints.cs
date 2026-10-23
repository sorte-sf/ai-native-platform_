namespace SorteFederation.Portal.EndPoints;

public sealed class ApplicationEmbeddingsEndPoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder group) => EntityEndpointBuilder.MapCrud(group, typeof(ApplicationEmbedding), "/application-embeddings", "ApplicationEmbedding");
}


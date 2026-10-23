
namespace SorteFederation.AppHost.Models;

public class ExternalServiceUrl
{
    public bool Enabled { get; set; }
    public List<ExternalApis> Apis { get; set; } = new List<ExternalApis>();
}

public class ExternalApis
{
    public required string Name { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string Authorization { get; set; }
    public required string BaseUrl { get; set; }
    public required string ApiUrl { get; set; }

}

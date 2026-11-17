using SorteFederation.Builders.Data.Domain;
using SorteFederation.EncryptEFCoreColumn;

namespace SorteFederation.Builders.Data.Entities;

public sealed class BuilderAiUserModel : BuilderEntity
{
    public string UserId { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    [EncryptColumn]
    public string ApiKey { get; set; } = string.Empty;
    public string AuthScheme { get; set; } = "bearer";
}

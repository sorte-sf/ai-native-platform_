using ANTSoftFederation.IdentityServer.DTOs.Base;

namespace ANTSoftFederation.IdentityServer.DTOs;

public class CreateIdentityProviderDto : BaseCreateDto
{
    public required Guid RealmId { get; set; }
    public required string Alias { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public bool TrustEmail { get; set; } = false;
    public bool StoreToken { get; set; } = false;
    public bool AddReadTokenRoleOnCreate { get; set; } = false;
    public bool LinkOnly { get; set; } = false;
    public string FirstBrokerLoginFlowAlias { get; set; } = string.Empty;
}

public class UpdateIdentityProviderDto : BaseUpdateDto
{
    public required Guid RealmId { get; set; }
    public required string Alias { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public bool TrustEmail { get; set; } = false;
    public bool StoreToken { get; set; } = false;
    public bool AddReadTokenRoleOnCreate { get; set; } = false;
    public bool LinkOnly { get; set; } = false;
    public string FirstBrokerLoginFlowAlias { get; set; } = string.Empty;
}

public class IdentityProviderDto : BaseDto
{
    public required Guid RealmId { get; set; }
    public required string Alias { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public bool TrustEmail { get; set; } = false;
    public bool StoreToken { get; set; } = false;
    public bool AddReadTokenRoleOnCreate { get; set; } = false;
    public bool LinkOnly { get; set; } = false;
    public string FirstBrokerLoginFlowAlias { get; set; } = string.Empty;
}

public class IdentityProviderListDto : BaseListDto
{
    public required string Alias { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
}
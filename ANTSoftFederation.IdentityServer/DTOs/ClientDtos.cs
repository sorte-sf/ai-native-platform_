
namespace ANTSoftFederation.IdentityServer.DTOs;

public class CreateClientDto : BaseCreateDto
{
    public required Guid ProjectId { get; set; }
    public required string ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public ClientProtocol Protocol { get; set; } = ClientProtocol.OpenIdConnect;
    public ClientAccessType AccessType { get; set; } = ClientAccessType.Public;
    public string ClientSecret { get; set; } = string.Empty;
    public bool ServiceAccountsEnabled { get; set; } = false;
    public bool AuthorizationServicesEnabled { get; set; } = false;
    public bool StandardFlowEnabled { get; set; } = true;
    public bool ImplicitFlowEnabled { get; set; } = false;
    public bool DirectAccessGrantsEnabled { get; set; } = true;
    public string RootUrl { get; set; } = string.Empty;
    public string AdminUrl { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public List<string> RedirectUris { get; set; } = new();
    public List<string> WebOrigins { get; set; } = new();
    public int AccessTokenLifespan { get; set; } = 300;
    public bool AlwaysDisplayInConsole { get; set; } = false;
    public bool ConsentRequired { get; set; } = false;
    public bool DisplayOnConsentScreen { get; set; } = true;
    public string ConsentScreenText { get; set; } = string.Empty;
    public bool PkceCodeChallengeMethod { get; set; } = false;
}

public class UpdateClientDto : BaseUpdateDto
{
    public required Guid ProjectId { get; set; }
    public required string ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public ClientProtocol Protocol { get; set; } = ClientProtocol.OpenIdConnect;
    public ClientAccessType AccessType { get; set; } = ClientAccessType.Public;
    public string ClientSecret { get; set; } = string.Empty;
    public bool ServiceAccountsEnabled { get; set; } = false;
    public bool AuthorizationServicesEnabled { get; set; } = false;
    public bool StandardFlowEnabled { get; set; } = true;
    public bool ImplicitFlowEnabled { get; set; } = false;
    public bool DirectAccessGrantsEnabled { get; set; } = true;
    public string RootUrl { get; set; } = string.Empty;
    public string AdminUrl { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public List<string> RedirectUris { get; set; } = new();
    public List<string> WebOrigins { get; set; } = new();
    public int AccessTokenLifespan { get; set; } = 300;
    public bool AlwaysDisplayInConsole { get; set; } = false;
    public bool ConsentRequired { get; set; } = false;
    public bool DisplayOnConsentScreen { get; set; } = true;
    public string ConsentScreenText { get; set; } = string.Empty;
    public bool PkceCodeChallengeMethod { get; set; } = false;
}

public class ClientDto : BaseDto
{
    public required Guid ProjectId { get; set; }
    public required string ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public ClientProtocol Protocol { get; set; } = ClientProtocol.OpenIdConnect;
    public ClientAccessType AccessType { get; set; } = ClientAccessType.Public;
    public string ClientSecret { get; set; } = string.Empty;
    public bool ServiceAccountsEnabled { get; set; } = false;
    public bool AuthorizationServicesEnabled { get; set; } = false;
    public bool StandardFlowEnabled { get; set; } = true;
    public bool ImplicitFlowEnabled { get; set; } = false;
    public bool DirectAccessGrantsEnabled { get; set; } = true;
    public string RootUrl { get; set; } = string.Empty;
    public string AdminUrl { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public List<string> RedirectUris { get; set; } = new();
    public List<string> WebOrigins { get; set; } = new();
    public int AccessTokenLifespan { get; set; } = 300;
    public bool AlwaysDisplayInConsole { get; set; } = false;
    public bool ConsentRequired { get; set; } = false;
    public bool DisplayOnConsentScreen { get; set; } = true;
    public string ConsentScreenText { get; set; } = string.Empty;
    public bool PkceCodeChallengeMethod { get; set; } = false;
}

public class ClientListDto : BaseListDto
{
    public required string ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
}
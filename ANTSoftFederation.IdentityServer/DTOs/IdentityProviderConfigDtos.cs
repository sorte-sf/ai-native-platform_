using ANTSoftFederation.IdentityServer.DTOs.Base;

namespace ANTSoftFederation.IdentityServer.DTOs;

public class CreateIdentityProviderConfigDto : BaseCreateDto
{
    public required Guid IdentityProviderId { get; set; }
    public required string ConfigKey { get; set; }
    public string ConfigValue { get; set; } = string.Empty;
}

public class UpdateIdentityProviderConfigDto : BaseUpdateDto
{
    public required Guid IdentityProviderId { get; set; }
    public required string ConfigKey { get; set; }
    public string ConfigValue { get; set; } = string.Empty;
}

public class IdentityProviderConfigDto : BaseDto
{
    public required Guid IdentityProviderId { get; set; }
    public required string ConfigKey { get; set; }
    public string ConfigValue { get; set; } = string.Empty;
}

public class IdentityProviderConfigListDto : BaseListDto
{
    public required string ConfigKey { get; set; }
    public string ConfigValue { get; set; } = string.Empty;
}
using ANTSoftFederation.IdentityServer.DTOs.Base;

namespace ANTSoftFederation.IdentityServer.DTOs;

public class CreateProtocolMapperConfigDto : BaseCreateDto
{
    public required Guid ProtocolMapperId { get; set; }
    public required string ConfigKey { get; set; }
    public string ConfigValue { get; set; } = string.Empty;
}

public class UpdateProtocolMapperConfigDto : BaseUpdateDto
{
    public required Guid ProtocolMapperId { get; set; }
    public required string ConfigKey { get; set; }
    public string ConfigValue { get; set; } = string.Empty;
}

public class ProtocolMapperConfigDto : BaseDto
{
    public required Guid ProtocolMapperId { get; set; }
    public required string ConfigKey { get; set; }
    public string ConfigValue { get; set; } = string.Empty;
}

public class ProtocolMapperConfigListDto : BaseListDto
{
    public required string ConfigKey { get; set; }
    public string ConfigValue { get; set; } = string.Empty;
}
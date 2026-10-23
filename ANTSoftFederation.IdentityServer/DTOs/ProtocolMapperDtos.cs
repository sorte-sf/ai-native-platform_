using ANTSoftFederation.IdentityServer.DTOs.Base;

namespace ANTSoftFederation.IdentityServer.DTOs;

public class CreateProtocolMapperDto : BaseCreateDto
{
    public required Guid ClientId { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Protocol { get; set; } = string.Empty;
    public string MapperType { get; set; } = string.Empty;
    public bool ConsentRequired { get; set; } = false;
    public string ConsentText { get; set; } = string.Empty;
}

public class UpdateProtocolMapperDto : BaseUpdateDto
{
    public required Guid ClientId { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Protocol { get; set; } = string.Empty;
    public string MapperType { get; set; } = string.Empty;
    public bool ConsentRequired { get; set; } = false;
    public string ConsentText { get; set; } = string.Empty;
}

public class ProtocolMapperDto : BaseDto
{
    public required Guid? ClientId { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Protocol { get; set; } = string.Empty;
    public string MapperType { get; set; } = string.Empty;
    public bool ConsentRequired { get; set; } = false;
    public string ConsentText { get; set; } = string.Empty;
}

public class ProtocolMapperListDto : BaseListDto
{
    public required string Name { get; set; }
    public string Protocol { get; set; } = string.Empty;
}
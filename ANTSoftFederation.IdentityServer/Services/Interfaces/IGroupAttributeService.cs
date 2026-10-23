using ANTSoftFederation.IdentityServer.DTOs;

namespace ANTSoftFederation.IdentityServer.Services.Interfaces;

public interface IGroupAttributeService
{
    Task<(IEnumerable<GroupAttributeListDto> GroupAttributes, int TotalCount)> GetGroupAttributesAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<GroupAttributeDto?> GetGroupAttributeByIdAsync(Guid id);
    
    Task<Guid> CreateGroupAttributeAsync(CreateGroupAttributeDto createDto);
    
    Task<bool> UpdateGroupAttributeAsync(Guid id, UpdateGroupAttributeDto updateDto);
    
    Task<bool> DeleteGroupAttributeAsync(Guid id);
}
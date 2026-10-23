using ANTSoftFederation.IdentityServer.DTOs;

namespace ANTSoftFederation.IdentityServer.Services.Interfaces;

public interface IUserAttributeService
{
    Task<(IEnumerable<UserAttributeListDto> UserAttributes, int TotalCount)> GetUserAttributesAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<UserAttributeDto?> GetUserAttributeByIdAsync(Guid userId, string name);
    
    Task<bool> CreateUserAttributeAsync(CreateUserAttributeDto createDto);
    
    Task<bool> UpdateUserAttributeAsync(Guid userId, string name, UpdateUserAttributeDto updateDto);
    
    Task<bool> DeleteUserAttributeAsync(Guid userId, string name);
}
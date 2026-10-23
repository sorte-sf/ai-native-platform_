using ANTSoftFederation.IdentityServer.DTOs;

namespace ANTSoftFederation.IdentityServer.Services.Interfaces;

public interface IUserSessionService
{
    Task<(IEnumerable<UserSessionListDto> UserSessions, int TotalCount)> GetUserSessionsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null);

    Task<UserSessionDto?> GetUserSessionByIdAsync(Guid userId, Guid projectId, string sessionId);
    
    Task<bool> CreateUserSessionAsync(CreateUserSessionDto createDto);
    
    Task<bool> UpdateUserSessionAsync(Guid userId, Guid projectId, string sessionId, UpdateUserSessionDto updateDto);
    
    Task<bool> DeleteUserSessionAsync(Guid userId, Guid projectId, string sessionId);
}
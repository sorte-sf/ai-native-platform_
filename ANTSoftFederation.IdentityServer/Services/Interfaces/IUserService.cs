
namespace ANTSoftFederation.IdentityServer.Services.Interfaces;

public interface IUserService
{
    Task<(IEnumerable<UserListDto> Users, int TotalCount)> GetUsersAsync(int page = 1, int pageSize = 10, string? search = null);

    Task<UserDto?> GetUserByIdAsync(Guid id);

    Task<Guid> CreateUserAsync(CreateUserDto createDto);

    Task<bool> UpdateUserAsync(Guid id, UpdateUserDto updateDto);

    Task<bool> DeleteUserAsync(Guid id);
}
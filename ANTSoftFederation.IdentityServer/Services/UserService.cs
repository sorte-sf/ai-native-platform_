
namespace ANTSoftFederation.IdentityServer.Services;

public class UserService : BaseService, IUserService
{
    private readonly ANTSoftIdentityDbContext _context;
    private readonly ISecurityService _securityService = new SecurityService(); 
    private readonly IMemoryCache _cache;

    public UserService(ANTSoftIdentityDbContext context, ISecurityService securityService, IMemoryCache cache)
    {
        _context = context;
        _securityService = securityService;
        _cache = cache;
    }

    public async Task<(IEnumerable<UserListDto> Users, int TotalCount)> GetUsersAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        var query = _context.Users.AsNoTracking().AsQueryable();
         
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u => u.Username.Contains(search) || u.Email.Contains(search) || u.DisplayName.Contains(search));
        }
         
        var totalCount = await query.CountAsync();

        // Apply pagination
        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserListDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                DisplayName = u.DisplayName,
                IsOnline = u.IsOnline,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                IsActive = u.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (users, totalCount);
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id)
    {
        var cacheKey = $"user_{id}";
        
        if (!_cache.TryGetValue(cacheKey, out UserDto? user))
        {
            user = await _context.Users.AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    DisplayName = u.DisplayName,
                    TelePhone = u.TelePhone,
                    EmailVerified = u.EmailVerified,
                    IsOnline = u.IsOnline,
                    PasswordChangedAt = u.PasswordChangedAt,
                    TemporaryPassword = u.TemporaryPassword,
                    TwoFactorEnabled = u.TwoFactorEnabled,
                    IsLockedOut = u.IsLockedOut,
                    AllowMultipleSessions = u.AllowMultipleSessions,
                    ChangePasswordOnFirstLogin = u.ChangePasswordOnFirstLogin,
                    TwoFactorType = u.TwoFactorType,
                    LastLoginDate = u.LastLoginDate,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    CreatedBy = u.CreatedBy,
                    UpdatedBy = u.UpdatedBy,
                    IsActive = u.RecordState == RecordState.Active
                })
                .FirstOrDefaultAsync();

            if (user != null)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));
                
                _cache.Set(cacheKey, user, cacheEntryOptions);
            }
        }

        return user;
    }

    public async Task<Guid> CreateUserAsync(CreateUserDto createDto)
    {
        var _passwordSalt = _securityService.GetpasswordSalt();
        var passwrd = _securityService.GetpasswordHash(_passwordSalt, createDto.Password);
        var user = new User
        {
            Username = createDto.Username,
            Email = createDto.Email,
            DisplayName = createDto.DisplayName,
            TelePhone = createDto.TelePhone,
            EmailVerified = createDto.EmailVerified,
            Password = passwrd,
            IsOnline = createDto.IsOnline,
            TemporaryPassword = createDto.TemporaryPassword,
            TwoFactorEnabled = createDto.TwoFactorEnabled,
            IsLockedOut = createDto.IsLockedOut,
            AllowMultipleSessions = createDto.AllowMultipleSessions,
            ChangePasswordOnFirstLogin = createDto.ChangePasswordOnFirstLogin,
            TwoFactorType = createDto.TwoFactorType,
            CreatedBy = createDto.CreatedBy
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user.Id;
    }

    public async Task<bool> UpdateUserAsync(Guid id, UpdateUserDto updateDto)
    {
        var user = await _context.Users.FindAsync(id);
        
        if (user == null)
            return false;

        user.Username = updateDto.Username;
        user.Email = updateDto.Email;
        user.DisplayName = updateDto.DisplayName;
        user.TelePhone = updateDto.TelePhone;
        user.EmailVerified = updateDto.EmailVerified;
        user.IsOnline = updateDto.IsOnline;
        user.TemporaryPassword = updateDto.TemporaryPassword;
        user.TwoFactorEnabled = updateDto.TwoFactorEnabled;
        user.IsLockedOut = updateDto.IsLockedOut;
        user.AllowMultipleSessions = updateDto.AllowMultipleSessions;
        user.ChangePasswordOnFirstLogin = updateDto.ChangePasswordOnFirstLogin;
        user.TwoFactorType = updateDto.TwoFactorType;
        user.UpdatedBy = updateDto.UpdatedBy;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        _cache.Remove($"user_{id}");
        
        return true;
    }

    /*
    public async Task<UserDetailResponseDto> GetUserDetailsByIdAsync(Guid userId)
    {
        try
        {
            var records = await _context.Users
                                 .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                                 .Include(u => u.UserProjects).ThenInclude(up => up.Project)
                                 .Include(u => u.UserGroups).ThenInclude(ug => ug.Group).ThenInclude(g => g.GroupRoles)
                                 .Where(u => u.Id == userId)
                                 .Select(u => new UserDetailResponseDto
                                 {
                                     Id = u.Id,
                                     Username = u.Username,
                                     Email = u.Email,
                                     DisplayName = u.DisplayName,
                                     IsLockedOut = u.IsLockedOut ?? false,
                                     EmailVerified = u.EmailVerified,
                                     CreatedAt = u.CreatedAt,
                                     LastLoginDate = u.LastLoginDate,
                                     UserRoles = u.UserRoles.Select(ur => new UserRoleDto
                                     {
                                         RoleId = ur.RoleId,
                                         RoleName = ur.Role.Name
                                     }).ToList(),
                                     UserProjects = u.UserProjects.Select(up => new UserProjectsDto
                                     {
                                         ProjectId = up.ProjectId,
                                         ProjectName = up.Project.Name,
                                         CreatedAt = up.CreatedAt
                                     }).ToList(),
                                     UserGroups = u.UserGroups.Select(ug => new UserGroupsDto
                                     {
                                         GroupId = ug.GroupId,
                                         GroupName = ug.Group.Name,
                                         GroupRoles = ug.Group.GroupRoles.Select(gr => new UserGroupRoleDto
                                         {
                                             UserGroupRoleId = gr.RoleId,
                                             UserGroupRoleName = gr.Role.Name,
                                             CreatedAt = gr.CreatedAt
                                         }).ToList()

                                     }).ToList()
                                 })
                                 .FirstOrDefaultAsync();

            return records;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while retrieving user details.", ex);
        }
    }
    */

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        
        if (user == null)
            return false;

        user.RecordState = RecordState.Deleted;
        user.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        _cache.Remove($"user_{id}");

        return true;
    }
}
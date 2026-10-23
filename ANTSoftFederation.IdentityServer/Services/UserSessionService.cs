
namespace ANTSoftFederation.IdentityServer.Services;

public class UserSessionService : BaseService, IUserSessionService
{
    private readonly ANTSoftIdentityDbContext _context;

    public UserSessionService(ANTSoftIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<UserSessionListDto> UserSessions, int TotalCount)> GetUserSessionsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null)
    {
        var query = _context.UserSessions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.SessionId.Contains(search));
        }

        var totalCount = await query.CountAsync();

        var userSessions = await query
            .OrderBy(x => x.SessionId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UserSessionListDto
            {
                Id = Guid.NewGuid(), // UserSession doesn't have a single Id, using a new Guid
                SessionId = x.SessionId,
                LastAccess = x.LastAccess,
                Started = x.Started,
                CreatedAt = x.Started,
                UpdatedAt = x.LastAccess,
                IsActive = x.LastAccess > DateTime.UtcNow.AddMinutes(-30) // Active if accessed within 30 minutes
            })
            .ToListAsync();

        return (userSessions, totalCount);
    }

    public async Task<UserSessionDto?> GetUserSessionByIdAsync(Guid userId, Guid projectId, string sessionId)
    {
        var userSession = await _context.UserSessions.FirstOrDefaultAsync(x => x.UserId == userId && x.ProjectId == projectId && x.SessionId == sessionId);

        if (userSession == null)
            return null;

        return new UserSessionDto
        {
            Id = Guid.NewGuid(), // UserSession doesn't have a single Id, using a new Guid
            UserId = userSession.UserId,
            ProjectId = userSession.ProjectId,
            SessionId = userSession.SessionId,
            IpAddress = userSession.IpAddress,
            LastAccess = userSession.LastAccess,
            Started = userSession.Started,
            UserAgent = userSession.UserAgent,
            CreatedAt = userSession.Started,
            UpdatedAt = userSession.LastAccess,
            IsActive = userSession.LastAccess > DateTime.UtcNow.AddMinutes(-30)
        };
    }

    public async Task<bool> CreateUserSessionAsync(CreateUserSessionDto createDto)
    {
        var userSession = new UserSession
        {
            UserId = createDto.UserId,
            ProjectId = createDto.ProjectId,
            SessionId = createDto.SessionId,
            IpAddress = createDto.IpAddress,
            LastAccess = createDto.LastAccess,
            Started = createDto.Started,
            UserAgent = createDto.UserAgent
        };

        _context.UserSessions.Add(userSession);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateUserSessionAsync(Guid userId, Guid projectId, string sessionId, UpdateUserSessionDto updateDto)
    {
        var userSession = await _context.UserSessions
            .FirstOrDefaultAsync(x => x.UserId == userId && x.ProjectId == projectId && x.SessionId == sessionId);

        if (userSession == null)
            return false;

        userSession.IpAddress = updateDto.IpAddress;
        userSession.LastAccess = updateDto.LastAccess;
        userSession.Started = updateDto.Started;
        userSession.UserAgent = updateDto.UserAgent;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserSessionAsync(Guid userId, Guid projectId, string sessionId)
    {
        var userSession = await _context.UserSessions
            .FirstOrDefaultAsync(x => x.UserId == userId && x.ProjectId == projectId && x.SessionId == sessionId);

        if (userSession == null)
            return false;

        _context.UserSessions.Remove(userSession);
        await _context.SaveChangesAsync();
        return true;
    }

}
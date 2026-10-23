 
namespace ANTSoftFederation.IdentityServer.Services;

public class UserProjectService : BaseService, IUserProjectService
{
    private readonly ANTSoftIdentityDbContext _context;

    public UserProjectService(ANTSoftIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<(ICollection<UserProjectListDto> UserProjects, int TotalCount)> GetUserProjectsAsync(int page, int pageSize, string? search)
    {
        var query = _context.UserProjects.AsQueryable();
         
        var totalCount = await query.CountAsync();

        var userProjects = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(up => new UserProjectListDto
            {
                UserId = up.UserId,
                ProjectId = up.ProjectId,
                ProjectName = up.Project.Name,
                CreatedAt = up.CreatedAt,
                UpdatedAt = up.UpdatedAt,
                IsActive = up.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (UserProjects: userProjects, TotalCount: totalCount);
    }

    public async Task<UserProjectDto?> GetUserProjectByIdAsync(Guid userId, Guid projectId)
    {
        var userProject = await _context.UserProjects
            .Where(up => up.UserId == userId && up.ProjectId == projectId)
            .Select(up => new UserProjectDto
            {
                UserId = up.UserId,
                ProjectId = up.ProjectId,
                ProjectName = up.Project.Name,
                CreatedAt = up.CreatedAt,
                UpdatedAt = up.UpdatedAt,
                CreatedBy = up.CreatedBy,
                UpdatedBy = up.UpdatedBy,
                IsActive = up.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return userProject;
    }

    public async Task<bool> CreateUserProjectAsync(CreateUserProjectDto createDto)
    { 
        var existing = await _context.UserProjects
            .AnyAsync(up => up.UserId == createDto.UserId && up.ProjectId == createDto.ProjectId);
            
        if (existing)
            return false;

        var userProject = new UserProject
        {
            UserId = createDto.UserId,
            ProjectId = createDto.ProjectId,
            CreatedBy = createDto.CreatedBy
        };

        _context.UserProjects.Add(userProject);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateUserProjectAsync(Guid userId, Guid projectId, UpdateUserProjectDto updateDto)
    {
        var userProject = await _context.UserProjects
            .FirstOrDefaultAsync(up => up.UserId == userId && up.ProjectId == projectId);
        
        if (userProject == null)
            return false;

        userProject.UserId = updateDto.UserId;
        userProject.ProjectId = updateDto.ProjectId;
        userProject.UpdatedBy = updateDto.UpdatedBy;
        userProject.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserProjectAsync(Guid userId, Guid projectId)
    {
        var userProject = await _context.UserProjects
            .FirstOrDefaultAsync(up => up.UserId == userId && up.ProjectId == projectId);
        
        if (userProject == null)
            return false;

        userProject.RecordState = RecordState.Deleted;
        userProject.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}
 
namespace ANTSoftFederation.IdentityServer.Services;

public class ProjectGroupService : BaseService, IProjectGroupService
{
    private readonly ANTSoftIdentityDbContext _context;

    public ProjectGroupService(ANTSoftIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<ProjectGroupListDto> ProjectGroups, int TotalCount)> GetProjectGroupsAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        var query = _context.ProjectGroups.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            // ProjectGroup has no searchable fields, so we'll skip search filter
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var projectGroups = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(pg => new ProjectGroupListDto
            {
                ProjectId = pg.ProjectId,
                GroupId = pg.GroupId,
                CreatedAt = pg.CreatedAt,
                UpdatedAt = pg.CreatedAt, // ProjectGroup doesn't have UpdatedAt, using CreatedAt
                IsActive = pg.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (projectGroups, totalCount);
    }

    public async Task<ProjectGroupDto?> GetProjectGroupByIdAsync(Guid projectId, Guid groupId)
    {
        var projectGroup = await _context.ProjectGroups
            .Where(pg => pg.ProjectId == projectId && pg.GroupId == groupId)
            .Select(pg => new ProjectGroupDto
            {
                ProjectId = pg.ProjectId,
                GroupId = pg.GroupId,
                CreatedAt = pg.CreatedAt,
                UpdatedAt = pg.CreatedAt, // ProjectGroup doesn't have UpdatedAt, using CreatedAt
                CreatedBy = string.Empty, // ProjectGroup doesn't have CreatedBy
                UpdatedBy = string.Empty, // ProjectGroup doesn't have UpdatedBy
                IsActive = pg.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return projectGroup;
    }

    public async Task<bool> CreateProjectGroupAsync(CreateProjectGroupDto createDto)
    {
        // Check if the ProjectGroup already exists
        var existing = await _context.ProjectGroups
            .AnyAsync(pg => pg.ProjectId == createDto.ProjectId && pg.GroupId == createDto.GroupId);
        
        if (existing)
            return false;

        var projectGroup = new ProjectGroup
        {
            ProjectId = createDto.ProjectId,
            GroupId = createDto.GroupId
            // Removed CreatedBy as ProjectGroup doesn't have this property
        };

        _context.ProjectGroups.Add(projectGroup);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateProjectGroupAsync(Guid projectId, Guid groupId, UpdateProjectGroupDto updateDto)
    {
        var projectGroup = await _context.ProjectGroups
            .FirstOrDefaultAsync(pg => pg.ProjectId == projectId && pg.GroupId == groupId);
        
        if (projectGroup == null)
            return false;

        projectGroup.ProjectId = updateDto.ProjectId;
        projectGroup.GroupId = updateDto.GroupId;
        // ProjectGroup doesn't have UpdatedBy or UpdatedAt properties
        // projectGroup.UpdatedBy = updateDto.UpdatedBy;
        // projectGroup.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteProjectGroupAsync(Guid projectId, Guid groupId)
    {
        var projectGroup = await _context.ProjectGroups
            .FirstOrDefaultAsync(pg => pg.ProjectId == projectId && pg.GroupId == groupId);
        
        if (projectGroup == null)
            return false;

        projectGroup.RecordState = RecordState.Deleted;
        // ProjectGroup doesn't have UpdatedAt property, but we can add it
        // projectGroup.UpdatedAt = DateTime.UtcNow;
            
        await _context.SaveChangesAsync();
        return true;
    }
}
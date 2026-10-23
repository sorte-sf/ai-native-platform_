
namespace SorteFederation.ANTApplications.Services;

public class ApplicationClaimService : BaseService, IApplicationClaimService
{
    private readonly ANTSoftPortalDbContext _context;

    public ApplicationClaimService(ANTSoftPortalDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<ApplicationClaimListDto> ApplicationClaims, int TotalCount)> GetApplicationClaimsAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        var query = _context.ApplicationClaims.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(ac => ac.Name.Contains(search) || ac.Value.Contains(search));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var applicationClaims = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ac => new ApplicationClaimListDto
            {
                Id = ac.Id,
                Name = ac.Name,
                Value = ac.Value,
                ParentId = ac.ParentId,
                ApplicationId = ac.ApplicationId,
                CreatedAt = ac.CreatedAt,
                UpdatedAt = ac.UpdatedAt,
                IsActive = ac.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (applicationClaims, totalCount);
    }

    public async Task<ApplicationClaimDto?> GetApplicationClaimByIdAsync(Guid id)
    {
        var applicationClaim = await _context.ApplicationClaims
            .Where(ac => ac.Id == id)
            .Select(ac => new ApplicationClaimDto
            {
                Id = ac.Id,
                Name = ac.Name,
                Value = ac.Value,
                ParentId = ac.ParentId,
                ApplicationId = ac.ApplicationId,
                CreatedAt = ac.CreatedAt,
                UpdatedAt = ac.UpdatedAt,
                CreatedBy = ac.CreatedBy,
                UpdatedBy = ac.UpdatedBy,
                IsActive = ac.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return applicationClaim;
    }

    public async Task<Guid> CreateApplicationClaimAsync(CreateApplicationClaimDto createDto)
    {
        var applicationClaim = new ApplicationClaim
        {
            Name = createDto.Name,
            Value = createDto.Value,
            ParentId = createDto.ParentId,
            ApplicationId = createDto.ApplicationId,
            CreatedBy = createDto.CreatedBy
        };

        _context.ApplicationClaims.Add(applicationClaim);
        await _context.SaveChangesAsync();

        return applicationClaim.Id;
    }

    public async Task<bool> UpdateApplicationClaimAsync(Guid id, UpdateApplicationClaimDto updateDto)
    {
        var applicationClaim = await _context.ApplicationClaims.FindAsync(id);
        
        if (applicationClaim == null)
            return false;

        applicationClaim.Name = updateDto.Name;
        applicationClaim.Value = updateDto.Value;
        applicationClaim.ParentId = updateDto.ParentId;
        applicationClaim.ApplicationId = updateDto.ApplicationId;
        applicationClaim.UpdatedBy = updateDto.UpdatedBy;
        applicationClaim.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteApplicationClaimAsync(Guid id)
    {
        var applicationClaim = await _context.ApplicationClaims.FindAsync(id);
        
        if (applicationClaim == null)
            return false;

        applicationClaim.RecordState = RecordState.Deleted;
        applicationClaim.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}

using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace SorteFederation.ANTApplications.Services;

public class ApplicationModuleIssueService : BaseService, IApplicationModuleIssueService
{
    private readonly ANTSoftPortalDbContext _context;

    public ApplicationModuleIssueService(ANTSoftPortalDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<ApplicationModuleIssueListDto> ApplicationModuleIssues, int TotalCount)> GetApplicationModuleIssuesAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null,
        float[]? searchVector = null)
    {
        var query = _context.ApplicationModuleIssues.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            if (searchVector != null && searchVector.Length > 0)
            {
                // Vector semantic search
                var vectorObj = new Vector(searchVector);
                var issueIds = await _context.ApplicationModuleIssueEmbeddings
                                    .OrderBy(e => e.IssueVector.CosineDistance(vectorObj))
                                    .Take(100)
                                    .Select(e => e.ApplicationModuleIssueId)
                                    .ToListAsync();
                                    
                if (issueIds.Any())
                {
                    query = query.Where(ami => issueIds.Contains(ami.Id));
                }
                else
                {
                    query = query.Where(ami => ami.Descriptions.Contains(search) || ami.Keywords.Contains(search));
                }
            }
            else 
            {
                // Fallback to text search
                query = query.Where(ami => ami.Descriptions.Contains(search) || ami.Keywords.Contains(search));
            }
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var applicationModuleIssues = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ami => new ApplicationModuleIssueListDto
            {
                Id = ami.Id,
                Descriptions = ami.Descriptions,
                Priority = ami.Priority,
                TicketStatus = ami.TicketStatus,
                TicketType = ami.TicketType,
                ApplicationName = ami.Application != null ? ami.Application.Name : string.Empty,
                CompanyName = ami.Company != null ? ami.Company.Name : string.Empty,
                ModuleName = ami.ApplicationModule != null ? ami.ApplicationModule.Name : string.Empty,
                CreatedAt = ami.CreatedAt,
                UpdatedAt = ami.UpdatedAt,
                IsActive = ami.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (applicationModuleIssues, totalCount);
    }

    public async Task<ApplicationModuleIssueDto?> GetApplicationModuleIssueByIdAsync(Guid id)
    {
        var applicationModuleIssue = await _context.ApplicationModuleIssues
            .Where(ami => ami.Id == id)
            .Select(ami => new ApplicationModuleIssueDto
            {
                Id = ami.Id,
                Descriptions = ami.Descriptions,
                PrId = ami.PrId,
                Solutions = ami.Solutions,
                Keywords = ami.Keywords,
                Priority = ami.Priority,
                ApplicationId = ami.ApplicationId,
                CompanyId = ami.CompanyId,
                ModuleId = ami.ModuleId,
                TicketStatus = ami.TicketStatus,
                TicketType = ami.TicketType,
                CustomerSatisfaction = ami.CustomerSatisfaction,
                ApplicationName = ami.Application != null ? ami.Application.Name : string.Empty,
                CompanyName = ami.Company != null ? ami.Company.Name : string.Empty,
                ModuleName = ami.ApplicationModule != null ? ami.ApplicationModule.Name : string.Empty,
                CreatedAt = ami.CreatedAt,
                UpdatedAt = ami.UpdatedAt,
                CreatedBy = ami.CreatedBy,
                UpdatedBy = ami.UpdatedBy,
                IsActive = ami.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return applicationModuleIssue;
    }

    public async Task<Guid> CreateApplicationModuleIssueAsync(CreateApplicationModuleIssueDto createDto)
    {
        var applicationModuleIssue = new ApplicationModuleIssue
        {
            Descriptions = createDto.Descriptions,
            PrId = createDto.PrId,
            Solutions = createDto.Solutions,
            Keywords = createDto.Keywords,
            Priority = createDto.Priority,
            ApplicationId = createDto.ApplicationId,
            CompanyId = createDto.CompanyId,
            ModuleId = createDto.ModuleId,
            TicketStatus = createDto.TicketStatus,
            TicketType = createDto.TicketType,
            CustomerSatisfaction = createDto.CustomerSatisfaction,
            CreatedBy = createDto.CreatedBy
        };

        _context.ApplicationModuleIssues.Add(applicationModuleIssue);
        await _context.SaveChangesAsync();

        return applicationModuleIssue.Id;
    }

    public async Task<bool> UpdateApplicationModuleIssueAsync(Guid id, UpdateApplicationModuleIssueDto updateDto)
    {
        var applicationModuleIssue = await _context.ApplicationModuleIssues.FindAsync(id);
        
        if (applicationModuleIssue == null)
            return false;

        applicationModuleIssue.Descriptions = updateDto.Descriptions;
        applicationModuleIssue.PrId = updateDto.PrId;
        applicationModuleIssue.Solutions = updateDto.Solutions;
        applicationModuleIssue.Keywords = updateDto.Keywords;
        applicationModuleIssue.Priority = updateDto.Priority;
        applicationModuleIssue.ApplicationId = updateDto.ApplicationId;
        applicationModuleIssue.CompanyId = updateDto.CompanyId;
        applicationModuleIssue.ModuleId = updateDto.ModuleId;
        applicationModuleIssue.TicketStatus = updateDto.TicketStatus;
        applicationModuleIssue.TicketType = updateDto.TicketType;
        applicationModuleIssue.CustomerSatisfaction = updateDto.CustomerSatisfaction;
        applicationModuleIssue.UpdatedBy = updateDto.UpdatedBy;
        applicationModuleIssue.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteApplicationModuleIssueAsync(Guid id)
    {
        var applicationModuleIssue = await _context.ApplicationModuleIssues.FindAsync(id);
        
        if (applicationModuleIssue == null)
            return false;

        applicationModuleIssue.RecordState = RecordState.Deleted;
        applicationModuleIssue.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}
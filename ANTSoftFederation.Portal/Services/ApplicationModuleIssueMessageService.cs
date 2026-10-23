 
namespace SorteFederation.ANTApplications.Services;

public class ApplicationModuleIssueMessageService : BaseService, IApplicationModuleIssueMessageService
{
    private readonly ANTSoftPortalDbContext _context;

    public ApplicationModuleIssueMessageService(ANTSoftPortalDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<ApplicationModuleIssueMessageListDto> ApplicationModuleIssueMessages, int TotalCount)> GetApplicationModuleIssueMessagesAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        var query = _context.ApplicationModuleIssueMessages.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(amim => amim.Message.Contains(search));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var applicationModuleIssueMessages = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(amim => new ApplicationModuleIssueMessageListDto
            {
                Id = amim.Id,
                Message = amim.Message,
                ApplicationModuleIssueId = amim.ApplicationModuleIssueId,
                IsCustomerMessage = amim.IsCustomerMessage,
                CreatedAt = amim.CreatedAt,
                UpdatedAt = amim.UpdatedAt,
                IsActive = amim.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (applicationModuleIssueMessages, totalCount);
    }

    public async Task<ApplicationModuleIssueMessageDto?> GetApplicationModuleIssueMessageByIdAsync(Guid id)
    {
        var applicationModuleIssueMessage = await _context.ApplicationModuleIssueMessages
            .Where(amim => amim.Id == id)
            .Select(amim => new ApplicationModuleIssueMessageDto
            {
                Id = amim.Id,
                Message = amim.Message,
                ApplicationModuleIssueId = amim.ApplicationModuleIssueId,
                IsCustomerMessage = amim.IsCustomerMessage,
                ParentMessageId = amim.ParentMessageId,
                CreatedAt = amim.CreatedAt,
                UpdatedAt = amim.UpdatedAt,
                CreatedBy = amim.CreatedBy,
                UpdatedBy = amim.UpdatedBy,
                IsActive = amim.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return applicationModuleIssueMessage;
    }

    public async Task<Guid> CreateApplicationModuleIssueMessageAsync(CreateApplicationModuleIssueMessageDto createDto)
    {
        var applicationModuleIssueMessage = new ApplicationModuleIssueMessage
        {
            Message = createDto.Message,
            ApplicationModuleIssueId = createDto.ApplicationModuleIssueId,
            IsCustomerMessage = createDto.IsCustomerMessage,
            ParentMessageId = createDto.ParentMessageId,
            CreatedBy = createDto.CreatedBy
        };

        _context.ApplicationModuleIssueMessages.Add(applicationModuleIssueMessage);
        await _context.SaveChangesAsync();

        return applicationModuleIssueMessage.Id;
    }

    public async Task<bool> UpdateApplicationModuleIssueMessageAsync(Guid id, UpdateApplicationModuleIssueMessageDto updateDto)
    {
        var applicationModuleIssueMessage = await _context.ApplicationModuleIssueMessages.FindAsync(id);
        
        if (applicationModuleIssueMessage == null)
            return false;

        applicationModuleIssueMessage.Message = updateDto.Message;
        applicationModuleIssueMessage.ApplicationModuleIssueId = updateDto.ApplicationModuleIssueId;
        applicationModuleIssueMessage.IsCustomerMessage = updateDto.IsCustomerMessage;
        applicationModuleIssueMessage.ParentMessageId = updateDto.ParentMessageId;
        applicationModuleIssueMessage.UpdatedBy = updateDto.UpdatedBy;
        applicationModuleIssueMessage.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteApplicationModuleIssueMessageAsync(Guid id)
    {
        var applicationModuleIssueMessage = await _context.ApplicationModuleIssueMessages.FindAsync(id);
        
        if (applicationModuleIssueMessage == null)
            return false;

        applicationModuleIssueMessage.RecordState = RecordState.Deleted;
        applicationModuleIssueMessage.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}
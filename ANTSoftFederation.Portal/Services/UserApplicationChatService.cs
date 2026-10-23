 

namespace SorteFederation.ANTApplications.Services;

public class UserApplicationChatService : BaseService, IUserApplicationChatService
{
    private readonly ANTSoftPortalDbContext _context;

    public UserApplicationChatService(ANTSoftPortalDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<UserApplicationChatListDto> UserApplicationChats, int TotalCount)> GetUserApplicationChatsAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        var query = _context.UserApplicationChats.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(uac => uac.Topic.Contains(search));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var userApplicationChats = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(uac => new UserApplicationChatListDto
            {
                Id = uac.Id,
                ParentId = uac.ParentId,
                Topic = uac.Topic,
                UserId = uac.UserId,  
                UpdatedAt = uac.UpdatedAt, 
            })
            .ToListAsync();

        return (userApplicationChats, totalCount);
    }

    public async Task<UserApplicationChatDto?> GetUserApplicationChatByIdAsync(Guid id)
    {
        var userApplicationChat = await _context.UserApplicationChats
            .Where(uac => uac.Id == id)
            .Select(uac => new UserApplicationChatDto
            {
                Id = uac.Id,
                ParentId = uac.ParentId,
                Topic = uac.Topic,
                ChatDetails = uac.ChatDetails,
                UserId = uac.UserId, 
                UpdatedAt = uac.UpdatedAt,
                CreatedBy = uac.CreatedBy,
                UpdatedBy = uac.UpdatedBy, 
            })
            .FirstOrDefaultAsync();

        return userApplicationChat;
    }

    public async Task<Guid> CreateUserApplicationChatAsync(CreateUserApplicationChatDto createDto)
    {
        var userApplicationChat = new UserApplicationChat
        {
            ParentId = createDto.ParentId,
            Topic = createDto.Topic,
            ChatDetails = createDto.ChatDetails,
            UserId = createDto.UserId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createDto.CreatedBy
        };

        _context.UserApplicationChats.Add(userApplicationChat);
        await _context.SaveChangesAsync();

        return userApplicationChat.Id;
    }

    public async Task<bool> UpdateUserApplicationChatAsync(Guid id, UpdateUserApplicationChatDto updateDto)
    {
        var userApplicationChat = await _context.UserApplicationChats.FindAsync(id);
        
        if (userApplicationChat == null)
            return false;

        userApplicationChat.ParentId = updateDto.ParentId;
        userApplicationChat.Topic = updateDto.Topic;
        userApplicationChat.ChatDetails = updateDto.ChatDetails;
        userApplicationChat.UserId = updateDto.UserId;
        userApplicationChat.UpdatedBy = updateDto.UpdatedBy;
        userApplicationChat.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserApplicationChatAsync(Guid id)
    {
        var userApplicationChat = await _context.UserApplicationChats.FindAsync(id);
        
        if (userApplicationChat == null)
            return false;

        //userApplicationChat.RecordState = RecordState.Deleted;
        userApplicationChat.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}
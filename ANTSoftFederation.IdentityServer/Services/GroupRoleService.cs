using ANTSoftFederation.IdentityServer.DataDbContext;
using ANTSoftFederation.IdentityServer.DTOs;
using ANTSoftFederation.IdentityServer.Services.Interfaces; 
using Microsoft.EntityFrameworkCore;

namespace ANTSoftFederation.IdentityServer.Services;

public class GroupRoleService : BaseService, IGroupRoleService
{
    private readonly ANTSoftIdentityDbContext _context;

    public GroupRoleService(ANTSoftIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<GroupRoleListDto> GroupRoles, int TotalCount)> GetGroupRolesAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        var query = _context.GroupRoles.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            // GroupRole has no searchable fields, so we'll skip search filter
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var groupRoles = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(gr => new GroupRoleListDto
            {
                GroupId = gr.GroupId,
                RoleId = gr.RoleId,
                CreatedAt = gr.CreatedAt,
                UpdatedAt = gr.UpdatedAt,
                IsActive = gr.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (groupRoles, totalCount);
    }

    public async Task<GroupRoleDto?> GetGroupRoleByIdAsync(Guid groupId, Guid roleId)
    {
        var groupRole = await _context.GroupRoles
            .Where(gr => gr.GroupId == groupId && gr.RoleId == roleId)
            .Select(gr => new GroupRoleDto
            {
                GroupId = gr.GroupId,
                RoleId = gr.RoleId,
                CreatedAt = gr.CreatedAt,
                UpdatedAt = gr.UpdatedAt,
                CreatedBy = gr.CreatedBy,
                UpdatedBy = gr.UpdatedBy,
                IsActive = gr.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return groupRole;
    }

    public async Task<bool> CreateGroupRoleAsync(CreateGroupRoleDto createDto)
    {
        // Check if the GroupRole already exists
        var existing = await _context.GroupRoles
            .AnyAsync(gr => gr.GroupId == createDto.GroupId && gr.RoleId == createDto.RoleId);
        
        if (existing)
            return false;

        var groupRole = new GroupRole
        {
            GroupId = createDto.GroupId,
            RoleId = createDto.RoleId,
            CreatedBy = createDto.CreatedBy
        };

        _context.GroupRoles.Add(groupRole);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateGroupRoleAsync(Guid groupId, Guid roleId, UpdateGroupRoleDto updateDto)
    {
        var groupRole = await _context.GroupRoles
            .FirstOrDefaultAsync(gr => gr.GroupId == groupId && gr.RoleId == roleId);
        
        if (groupRole == null)
            return false;

        groupRole.GroupId = updateDto.GroupId;
        groupRole.RoleId = updateDto.RoleId;
        groupRole.UpdatedBy = updateDto.UpdatedBy;
        groupRole.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteGroupRoleAsync(Guid groupId, Guid roleId)
    {
        var groupRole = await _context.GroupRoles
            .FirstOrDefaultAsync(gr => gr.GroupId == groupId && gr.RoleId == roleId);
        
        if (groupRole == null)
            return false;

        groupRole.RecordState = RecordState.Deleted;
        groupRole.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}
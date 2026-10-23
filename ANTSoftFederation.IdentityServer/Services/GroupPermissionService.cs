using ANTSoftFederation.IdentityServer.DataDbContext;
using ANTSoftFederation.IdentityServer.DTOs;
using ANTSoftFederation.IdentityServer.Services.Interfaces; 
using Microsoft.EntityFrameworkCore;

namespace ANTSoftFederation.IdentityServer.Services;

public class GroupPermissionService : BaseService, IGroupPermissionService
{
    private readonly ANTSoftIdentityDbContext _context;

    public GroupPermissionService(ANTSoftIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<GroupPermissionListDto> GroupPermissions, int TotalCount)> GetGroupPermissionsAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        var query = _context.GroupPermissions.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            // GroupPermission has no searchable fields, so we'll skip search filter
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var groupPermissions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(gp => new GroupPermissionListDto
            {
                Id = gp.Id,
                GroupId = gp.GroupId,
                PermissionId = gp.PermissionId,
                CreatedAt = gp.CreatedAt,
                UpdatedAt = gp.UpdatedAt,
                IsActive = gp.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (groupPermissions, totalCount);
    }

    public async Task<GroupPermissionDto?> GetGroupPermissionByIdAsync(Guid groupId, Guid permissionId)
    {
        var groupPermission = await _context.GroupPermissions
            .Where(gp => gp.GroupId == groupId && gp.PermissionId == permissionId)
            .Select(gp => new GroupPermissionDto
            {
                Id = gp.Id,
                GroupId = gp.GroupId,
                PermissionId = gp.PermissionId,
                CreatedAt = gp.CreatedAt,
                UpdatedAt = gp.UpdatedAt,
                CreatedBy = gp.CreatedBy,
                UpdatedBy = gp.UpdatedBy,
                IsActive = gp.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return groupPermission;
    }

    public async Task<bool> CreateGroupPermissionAsync(CreateGroupPermissionDto createDto)
    {
        var groupPermission = new GroupPermission
        {
            GroupId = createDto.GroupId,
            PermissionId = createDto.PermissionId,
            CreatedBy = createDto.CreatedBy
        };

        _context.GroupPermissions.Add(groupPermission);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateGroupPermissionAsync(Guid groupId, Guid permissionId, UpdateGroupPermissionDto updateDto)
    {
        var groupPermission = await _context.GroupPermissions
            .FirstOrDefaultAsync(gp => gp.GroupId == groupId && gp.PermissionId == permissionId);
        
        if (groupPermission == null)
            return false;

        groupPermission.GroupId = updateDto.GroupId;
        groupPermission.PermissionId = updateDto.PermissionId;
        groupPermission.UpdatedBy = updateDto.UpdatedBy;
        groupPermission.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteGroupPermissionAsync(Guid groupId, Guid permissionId)
    {
        var groupPermission = await _context.GroupPermissions
            .FirstOrDefaultAsync(gp => gp.GroupId == groupId && gp.PermissionId == permissionId);
        
        if (groupPermission == null)
            return false;

        groupPermission.RecordState = RecordState.Deleted;
        groupPermission.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}
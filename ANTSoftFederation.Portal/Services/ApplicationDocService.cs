
using SorteFederation.ANTApplications.Services.Base;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace SorteFederation.ANTApplications.Services;

public class ApplicationDocService : BaseService, IApplicationDocService
{
    private readonly ANTSoftPortalDbContext _context;

    public ApplicationDocService(ANTSoftPortalDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<ApplicationDocListDto> ApplicationDocs, int TotalCount)> GetApplicationDocsAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null,
        float[]? searchVector = null)
    {
        var query = _context.ApplicationDocs.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
             if (searchVector != null && searchVector.Length > 0)
             {
                 // Vector semantic search
                 var vectorObj = new Vector(searchVector);
                 var docIds = await _context.ApplicationEmbeddings
                                     .OrderBy(e => e.AppVector.CosineDistance(vectorObj))
                                     .Take(100)
                                     .Select(e => e.ApplicationDocId)
                                     .ToListAsync();
                                     
                 if (docIds.Any())
                 {
                     query = query.Where(ad => docIds.Contains(ad.Id));
                 }
                 else
                 {
                     query = query.Where(ad => ad.Descriptions.Contains(search) || ad.FileName.Contains(search));
                 }
             }
             else 
             {
                 // Fallback to basic text search
                 query = query.Where(ad => ad.Descriptions.Contains(search) || ad.FileName.Contains(search));
             }
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var applicationDocs = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ad => new ApplicationDocListDto
            {
                Id = ad.Id,
                Type = ad.Type,
                Descriptions = ad.Descriptions,
                FileName = ad.FileName,
                ApplicationId = ad.ApplicationId,
                CreatedAt = ad.CreatedAt,
                UpdatedAt = ad.UpdatedAt,
                IsActive = ad.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (applicationDocs, totalCount);
    }

    public async Task<ApplicationDocDto?> GetApplicationDocByIdAsync(Guid id)
    {
        var applicationDoc = await _context.ApplicationDocs
            .Where(ad => ad.Id == id)
            .Select(ad => new ApplicationDocDto
            {
                Id = ad.Id,
                Type = ad.Type,
                Descriptions = ad.Descriptions,
                FileName = ad.FileName,
                FileLink = ad.FileLink,
                PhysicalFilePath = ad.PhysicalFilePath,
                ParentId = ad.ParentId,
                ReviewedById = ad.ReviewedById,
                ApplicationId = ad.ApplicationId,
                AppVersionId = ad.AppVersionId,
                CreatedAt = ad.CreatedAt,
                UpdatedAt = ad.UpdatedAt,
                CreatedBy = ad.CreatedBy,
                UpdatedBy = ad.UpdatedBy,
                IsActive = ad.RecordState == RecordState.Active
            })
            .FirstOrDefaultAsync();

        return applicationDoc;
    }

    public async Task<Guid> CreateApplicationDocAsync(CreateApplicationDocDto createDto)
    {
        var applicationDoc = new ApplicationDoc
        {
            Type = createDto.Type,
            Descriptions = createDto.Descriptions,
            FileName = createDto.FileName,
            FileLink = createDto.FileLink,
            PhysicalFilePath = createDto.PhysicalFilePath,
            ParentId = createDto.ParentId,
            ReviewedById = createDto.ReviewedById,
            ApplicationId = createDto.ApplicationId,
            AppVersionId = createDto.AppVersionId,
            CreatedBy = createDto.CreatedBy
        };

        _context.ApplicationDocs.Add(applicationDoc);
        await _context.SaveChangesAsync();

        return applicationDoc.Id;
    }

    public async Task<bool> UpdateApplicationDocAsync(Guid id, UpdateApplicationDocDto updateDto)
    {
        var applicationDoc = await _context.ApplicationDocs.FindAsync(id);
        
        if (applicationDoc == null)
            return false;

        applicationDoc.Type = updateDto.Type;
        applicationDoc.Descriptions = updateDto.Descriptions;
        applicationDoc.FileName = updateDto.FileName;
        applicationDoc.FileLink = updateDto.FileLink;
        applicationDoc.PhysicalFilePath = updateDto.PhysicalFilePath;
        applicationDoc.ParentId = updateDto.ParentId;
        applicationDoc.ReviewedById = updateDto.ReviewedById;
        applicationDoc.ApplicationId = updateDto.ApplicationId;
        applicationDoc.AppVersionId = updateDto.AppVersionId;
        applicationDoc.UpdatedBy = updateDto.UpdatedBy;
        applicationDoc.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteApplicationDocAsync(Guid id)
    {
        var applicationDoc = await _context.ApplicationDocs.FindAsync(id);
        
        if (applicationDoc == null)
            return false;

        applicationDoc.RecordState = RecordState.Deleted;
        applicationDoc.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}
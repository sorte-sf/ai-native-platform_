using SorteFederation.Builders.Contracts;
using SorteFederation.Builders.Data;
using SorteFederation.Builders.Data.Domain;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;
namespace SorteFederation.Builders.Services;
public sealed class TenantRelationshipService(BuilderDbContext metadata, TenantRuntimeContextFactory factory,
    BuilderAuthorizationService authorization, IHttpContextAccessor accessor)
{
    public async Task<BuilderRecordRelationship> Create(CreateRecordRelationshipRequest request, CancellationToken token)
    {
        var relationship = await metadata.Relationships.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == request.RelationshipId && x.TenantId == request.TenantId, token)
            ?? throw new KeyNotFoundException("Relationship was not found.");
        var user = CurrentUser();
        await using var db = await factory.CreateAsync(request.TenantId, token);
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            // A transient failure may retry after a rollback; discard any entities tracked
            // by the failed attempt before checking the relationship again.
            db.ChangeTracker.Clear();
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, token);
            var sourceExists = await db.Records.AnyAsync(x => x.Id == request.SourceRecordId && x.TenantId == request.TenantId && x.ObjectId == relationship.SourceObjectId, token);
            var targetExists = await db.Records.AnyAsync(x => x.Id == request.TargetRecordId && x.TenantId == request.TenantId && x.ObjectId == relationship.TargetObjectId, token);
            if (!sourceExists || !targetExists)
                throw new ArgumentException("Records must belong to the relationship's source and target tables.");
            await authorization.EnsureRecordAccess(request.TenantId, relationship.SourceObjectId, request.SourceRecordId, "update", user, token);
            await authorization.EnsureRecordAccess(request.TenantId, relationship.TargetObjectId, request.TargetRecordId, "update", user, token);

            var existing = await db.RecordRelationships.SingleOrDefaultAsync(x => x.TenantId == request.TenantId &&
                x.RelationshipId == relationship.Id && x.SourceRecordId == request.SourceRecordId && x.TargetRecordId == request.TargetRecordId, token);
            if (existing is not null)
            {
                await transaction.CommitAsync(token);
                return existing;
            }

            var cardinality = RelationshipCardinalityRules.Normalize(relationship.Cardinality);
            if (RelationshipCardinalityRules.SourceIsUnique(cardinality) &&
                await db.RecordRelationships.AnyAsync(x => x.RelationshipId == relationship.Id && x.SourceRecordId == request.SourceRecordId, token))
                throw new InvalidOperationException("This source record already has the maximum number of links for the relationship.");
            if (RelationshipCardinalityRules.TargetIsUnique(cardinality) &&
                await db.RecordRelationships.AnyAsync(x => x.RelationshipId == relationship.Id && x.TargetRecordId == request.TargetRecordId, token))
                throw new InvalidOperationException("This target record already has the maximum number of links for the relationship.");

            var entity = new BuilderRecordRelationship { TenantId = request.TenantId, RelationshipId = relationship.Id,
                SourceRecordId = request.SourceRecordId, TargetRecordId = request.TargetRecordId };
            db.RecordRelationships.Add(entity);
            try
            {
                await db.SaveChangesAsync(token);
                await transaction.CommitAsync(token);
            }
            catch (Npgsql.PostgresException error) when (error.SqlState == "40001")
            {
                throw new InvalidOperationException("The relationship changed concurrently. Please retry.", error);
            }
            return entity;
        });
    }

    public async Task<IReadOnlyList<BuilderRecordRelationship>> List(Guid tenantId, Guid relationshipId, Guid? sourceRecordId, CancellationToken token)
    {
        var relationship = await metadata.Relationships.AsNoTracking().SingleOrDefaultAsync(x => x.Id == relationshipId && x.TenantId == tenantId, token)
            ?? throw new KeyNotFoundException("Relationship was not found.");
        var user = CurrentUser();
        await using var db = await factory.CreateAsync(tenantId, token);
        var query = db.RecordRelationships.AsNoTracking().Where(x => x.RelationshipId == relationshipId && x.TenantId == tenantId);
        if (sourceRecordId.HasValue) query = query.Where(x => x.SourceRecordId == sourceRecordId.Value);
        var links = await query.OrderBy(x => x.CreatedAt).ToListAsync(token);
        var accessible = new List<BuilderRecordRelationship>(links.Count);
        foreach (var link in links)
        {
            try
            {
                await authorization.EnsureRecordAccess(tenantId, relationship.SourceObjectId, link.SourceRecordId, "read", user, token);
                await authorization.EnsureRecordAccess(tenantId, relationship.TargetObjectId, link.TargetRecordId, "read", user, token);
                accessible.Add(link);
            }
            catch (UnauthorizedAccessException) { /* Do not disclose links to records outside the user's grant. */ }
        }
        return accessible;
    }

    public async Task<bool> Delete(Guid tenantId, Guid id, CancellationToken token)
    {
        await using var db = await factory.CreateAsync(tenantId, token);
        var entity = await db.RecordRelationships.SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, token);
        if (entity is null) return false;
        var relationship = await metadata.Relationships.AsNoTracking().SingleOrDefaultAsync(x => x.Id == entity.RelationshipId && x.TenantId == tenantId, token)
            ?? throw new KeyNotFoundException("Relationship was not found.");
        var user = CurrentUser();
        await authorization.EnsureRecordAccess(tenantId, relationship.SourceObjectId, entity.SourceRecordId, "update", user, token);
        await authorization.EnsureRecordAccess(tenantId, relationship.TargetObjectId, entity.TargetRecordId, "update", user, token);
        db.RecordRelationships.Remove(entity);
        await db.SaveChangesAsync(token);
        return true;
    }

    private ClaimsPrincipal CurrentUser() => accessor.HttpContext?.User ?? new ClaimsPrincipal();
}

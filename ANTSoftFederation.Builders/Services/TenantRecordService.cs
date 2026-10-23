using SorteFederation.Builders.Data.Domain;

namespace SorteFederation.Builders.Services;

public sealed class TenantRecordService(
    BuilderDbContext metadata,
    TenantRuntimeContextFactory factory,
    FormRuntimeService forms,
    BuilderAuthorizationService authorization,
    AuditService audit,
    IHttpContextAccessor accessor,
    RuleDefinitionService rules,
    PurchaseOrganizationService purchaseOrganization)
{
    public async Task<BuilderRecord> Create(CreateRecordRequest request, CancellationToken token)
    {
        var user = accessor.HttpContext?.User ?? new ClaimsPrincipal();
        await authorization.EnsureObjectAccess(request.TenantId, request.ObjectId, "create", user, token);
        var objectKey = await metadata.Objects.AsNoTracking().Where(x => x.Id == request.ObjectId && x.TenantId == request.TenantId)
            .Select(x => x.Key).SingleOrDefaultAsync(token);
        var dataJson = await forms.PrepareForWrite(request.TenantId, request.ObjectId, request.DataJson, token);
        if (PurchaseApprovalRecordRules.IsRequestObject(objectKey)) dataJson = PurchaseApprovalRecordRules.ForceDraftStatus(dataJson, "status");
        await purchaseOrganization.ValidateAsync(request.TenantId, request.ObjectId, dataJson, null, null, token);
        var valid = await forms.Validate(new ValidateFormRequest(request.TenantId, request.ObjectId, dataJson), user, token);
        if (!valid.Valid) throw new ArgumentException(string.Join(" ", valid.Errors));
        await using var runtime = await factory.CreateAsync(request.TenantId, token);
        var record = new BuilderRecord { TenantId = request.TenantId, ObjectId = request.ObjectId, DataJson = dataJson };
        runtime.Records.Add(record);
        await runtime.SaveChangesAsync(token);
        await audit.Record(new CreateAuditEventRequest(request.TenantId, "record.created", "record", record.Id, null), user,
            accessor.HttpContext?.TraceIdentifier, token);
        await rules.TriggerAsync(request.TenantId, request.ObjectId, "record.created", record.Id, dataJson, user, token);
        return record;
    }

    public async Task<BuilderRecord?> Get(Guid tenantId, Guid id, CancellationToken token)
    {
        await using var runtime = await factory.CreateAsync(tenantId, token);
        var record = await runtime.Records.AsNoTracking().SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, token);
        if (record is null) return null;
        var user = accessor.HttpContext?.User;
        await authorization.EnsureRecordAccess(tenantId, record.ObjectId, record.Id, "read", user ?? new ClaimsPrincipal(), token);
        record.DataJson = await forms.FilterReadableAsync(tenantId, record.ObjectId, record.DataJson, user, token);
        return record;
    }

    public async Task<List<BuilderRecord>> Query(Guid tenantId, Guid objectId, string? search, CancellationToken token)
    {
        var user = accessor.HttpContext?.User ?? new ClaimsPrincipal();
        await authorization.EnsureObjectAccess(tenantId, objectId, "read", user, token);
        await using var runtime = await factory.CreateAsync(tenantId, token);
        var records = await runtime.Records.AsNoTracking().Where(x => x.TenantId == tenantId && x.ObjectId == objectId)
            .OrderByDescending(x => x.UpdatedAt).Take(1000).ToListAsync(token);
        records = await authorization.FilterRecords(tenantId, objectId, "read", records, user, token);
        foreach (var record in records)
            record.DataJson = await forms.FilterReadableAsync(tenantId, record.ObjectId, record.DataJson, user, token);
        if (!string.IsNullOrWhiteSpace(search)) records = records.Where(x => x.DataJson.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        return records;
    }

    public async Task<BuilderRecord> Update(Guid tenantId, Guid id, UpdateRecordRequest request, CancellationToken token)
    {
        var user = accessor.HttpContext?.User ?? new ClaimsPrincipal();
        await using var runtime = await factory.CreateAsync(tenantId, token);
        var record = await runtime.Records.SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, token)
            ?? throw new KeyNotFoundException("Record was not found.");
        await authorization.EnsureRecordAccess(tenantId, record.ObjectId, record.Id, "update", user, token);
        if (record.Version != request.Version) throw new InvalidOperationException("The record was changed by another request.");
        var originalJson = record.DataJson;
        var objectKey = await metadata.Objects.AsNoTracking().Where(x => x.Id == record.ObjectId && x.TenantId == tenantId)
            .Select(x => x.Key).SingleOrDefaultAsync(token);
        var mergedJson = await forms.PreserveUnwritableFieldsAsync(tenantId, record.ObjectId, originalJson, request.DataJson, user, token);
        var dataJson = await forms.PrepareForWrite(tenantId, record.ObjectId, mergedJson, token);
        if (PurchaseApprovalRecordRules.IsRequestObject(objectKey)) PurchaseApprovalRecordRules.EnsureStatusUnchanged(originalJson, dataJson, "status");
        await purchaseOrganization.ValidateAsync(tenantId, record.ObjectId, dataJson, originalJson, record.Id, token);
        var valid = await forms.Validate(new ValidateFormRequest(tenantId, record.ObjectId, dataJson), user, token, originalJson);
        if (!valid.Valid) throw new ArgumentException(string.Join(" ", valid.Errors));
        record.DataJson = dataJson;
        record.Version++;
        record.UpdatedAt = DateTimeOffset.UtcNow;
        await runtime.SaveChangesAsync(token);
        await audit.Record(new CreateAuditEventRequest(tenantId, "record.updated", "record", record.Id, null), user,
            accessor.HttpContext?.TraceIdentifier, token);
        await rules.TriggerAsync(tenantId, record.ObjectId, "record.updated", record.Id, dataJson, user, token);
        return record;
    }

    public async Task<bool> Delete(Guid tenantId, Guid id, CancellationToken token)
    {
        var user = accessor.HttpContext?.User ?? new ClaimsPrincipal();
        await using var runtime = await factory.CreateAsync(tenantId, token);
        var record = await runtime.Records.SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, token);
        if (record is null) return false;
        await authorization.EnsureRecordAccess(tenantId, record.ObjectId, record.Id, "delete", user, token);
        runtime.Records.Remove(record);
        await runtime.SaveChangesAsync(token);
        await audit.Record(new CreateAuditEventRequest(tenantId, "record.deleted", "record", record.Id, null), user,
            accessor.HttpContext?.TraceIdentifier, token);
        return true;
    }
}

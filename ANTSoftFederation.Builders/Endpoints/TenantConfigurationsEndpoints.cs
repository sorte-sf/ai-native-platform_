using SorteFederation.Builders.Data.Domain;

namespace SorteFederation.Builders.Endpoints;

public sealed class TenantConfigurationsEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/tenant-configurations").WithTags("Tenant Configurations").RequireAuthorization();

        group.MapGet("/tenants", async (ClaimsPrincipal user, BuilderDbContext db, CancellationToken token) =>
        {
            if (!IsAdmin(user)) return Results.Forbid();
            var query = db.Tenants.AsNoTracking().Where(x => x.RecordState != RecordState.Deleted);
            if (!user.IsInRole(BuilderRoleNames.SystemAdmin))
            {
                if (!TryTenant(user, out var tenantId)) return Results.Forbid();
                query = query.Where(x => x.TenantId == tenantId);
            }
            return Results.Ok(await query.OrderBy(x => x.Name).Select(x => new TenantDto(x.TenantId, x.Name, x.RecordState)).ToListAsync(token));
        });

        group.MapGet("/tenants/{tenantId:guid}/smtp", async (Guid tenantId, ClaimsPrincipal user, BuilderDbContext db, CancellationToken token) =>
        {
            if (!TryResolveTenantId(user, tenantId, out var currentTenantId)) return Results.Forbid();
            tenantId = currentTenantId;
            if (!await TenantIsActive(db, tenantId, token)) return Results.NotFound("Tenant not found.");
            var smtp = await db.TenantSmtpConfigurations.AsNoTracking().SingleOrDefaultAsync(x => x.TenantId == tenantId, token);
            return smtp is null ? Results.NoContent() : Results.Ok(ToSmtpDto(smtp));
        });

        group.MapPut("/tenants/{tenantId:guid}/smtp", async (Guid tenantId, SaveSmtpRequest body, ClaimsPrincipal user, BuilderDbContext db, CancellationToken token) =>
        {
            if (!TryResolveTenantId(user, tenantId, out var currentTenantId)) return Results.Forbid();
            tenantId = currentTenantId;
            if (string.IsNullOrWhiteSpace(body.Host) || string.IsNullOrWhiteSpace(body.From) || body.Port is < 1 or > 65535)
                return Results.BadRequest("SMTP host, sender address, and a valid port are required.");
            if (!await db.Tenants.AnyAsync(x => x.TenantId == tenantId && x.RecordState != RecordState.Deleted, token)) return Results.NotFound("Tenant not found.");
            var smtp = await db.TenantSmtpConfigurations.SingleOrDefaultAsync(x => x.TenantId == tenantId, token);
            if (smtp is null) { smtp = new TenantSmtpConfiguration { TenantId = tenantId }; db.TenantSmtpConfigurations.Add(smtp); }
            smtp.Host = body.Host.Trim(); smtp.Port = body.Port; smtp.From = body.From.Trim(); smtp.DisplayName = body.DisplayName?.Trim();
            smtp.UserName = body.UserName?.Trim(); smtp.EnableSsl = body.EnableSsl; smtp.UseStartTls = body.UseStartTls;
            if (body.ClearPassword) smtp.Password = null;
            else if (!string.IsNullOrWhiteSpace(body.Password)) smtp.Password = body.Password;
            smtp.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(token);
            return Results.Ok(ToSmtpDto(smtp));
        });

        group.MapDelete("/tenants/{tenantId:guid}/smtp", async (Guid tenantId, ClaimsPrincipal user, BuilderDbContext db, CancellationToken token) =>
        {
            if (!TryResolveTenantId(user, tenantId, out var currentTenantId)) return Results.Forbid();
            tenantId = currentTenantId;
            if (!await TenantIsActive(db, tenantId, token)) return Results.NotFound("Tenant not found.");
            var smtp = await db.TenantSmtpConfigurations.SingleOrDefaultAsync(x => x.TenantId == tenantId, token);
            if (smtp is null) return Results.NotFound();
            db.TenantSmtpConfigurations.Remove(smtp);
            await db.SaveChangesAsync(token);
            return Results.NoContent();
        });

        group.MapGet("/tenants/{tenantId:guid}/integrations", async (Guid tenantId, ClaimsPrincipal user, BuilderDbContext db, CancellationToken token) =>
        {
            if (!TryResolveTenantId(user, tenantId, out var currentTenantId)) return Results.Forbid();
            tenantId = currentTenantId;
            if (!await TenantIsActive(db, tenantId, token)) return Results.NotFound("Tenant not found.");
            return Results.Ok(await db.TenantIntegrationConfigurations.AsNoTracking().Where(x => x.TenantId == tenantId)
                .OrderBy(x => x.Name).Select(x => new IntegrationDto(x.Id, x.TenantId, x.ApplicationId, x.Name, x.IntegrationType,
                    x.Port, x.UserName, x.DatabaseName, x.EnableSsl, !string.IsNullOrEmpty(x.Password),
                    !string.IsNullOrEmpty(x.Token), !string.IsNullOrEmpty(x.ConnectionString), x.TenantTableMappings.Count))
                .ToListAsync(token));
        });

        group.MapPost("/tenants/{tenantId:guid}/integrations", async (Guid tenantId, SaveIntegrationRequest body, ClaimsPrincipal user, BuilderDbContext db, CancellationToken token) =>
        {
            if (!TryResolveTenantId(user, tenantId, out var currentTenantId)) return Results.Forbid();
            tenantId = currentTenantId;
            var error = await ValidateIntegration(body, tenantId, db, token, null);
            if (error is not null) return Results.BadRequest(error);
            var entity = new TenantIntegrationConfiguration { TenantId = tenantId };
            ApplyIntegration(entity, body);
            db.TenantIntegrationConfigurations.Add(entity);
            await db.SaveChangesAsync(token);
            return Results.Created($"/builder/tenant-configurations/tenants/{tenantId}/integrations/{entity.Id}", ToIntegrationDto(entity));
        });

        group.MapPut("/tenants/{tenantId:guid}/integrations/{integrationId:guid}", async (Guid tenantId, Guid integrationId, SaveIntegrationRequest body, ClaimsPrincipal user, BuilderDbContext db, CancellationToken token) =>
        {
            if (!TryResolveTenantId(user, tenantId, out var currentTenantId)) return Results.Forbid();
            tenantId = currentTenantId;
            var error = await ValidateIntegration(body, tenantId, db, token, integrationId);
            if (error is not null) return Results.BadRequest(error);
            var entity = await db.TenantIntegrationConfigurations.SingleOrDefaultAsync(x => x.Id == integrationId && x.TenantId == tenantId, token);
            if (entity is null) return Results.NotFound();
            ApplyIntegration(entity, body);
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(token);
            return Results.Ok(ToIntegrationDto(entity));
        });

        group.MapDelete("/tenants/{tenantId:guid}/integrations/{integrationId:guid}", async (Guid tenantId, Guid integrationId, ClaimsPrincipal user, BuilderDbContext db, CancellationToken token) =>
        {
            if (!TryResolveTenantId(user, tenantId, out var currentTenantId)) return Results.Forbid();
            tenantId = currentTenantId;
            var entity = await db.TenantIntegrationConfigurations.SingleOrDefaultAsync(x => x.Id == integrationId && x.TenantId == tenantId, token);
            if (entity is null) return Results.NotFound();
            db.TenantIntegrationConfigurations.Remove(entity);
            await db.SaveChangesAsync(token);
            return Results.NoContent();
        });

        group.MapGet("/tenants/{tenantId:guid}/integrations/{integrationId:guid}/mappings", async (Guid tenantId, Guid integrationId, ClaimsPrincipal user, BuilderDbContext db, CancellationToken token) =>
        {
            if (!TryResolveTenantId(user, tenantId, out var currentTenantId)) return Results.Forbid();
            tenantId = currentTenantId;
            if (!await TenantIsActive(db, tenantId, token)) return Results.NotFound("Tenant not found.");
            if (!await db.TenantIntegrationConfigurations.AnyAsync(x => x.Id == integrationId && x.TenantId == tenantId, token)) return Results.NotFound();
            return Results.Ok(await db.TenantTableMappings.AsNoTracking().Where(x => x.TenantId == tenantId && x.TenantIntegrationConfigurationId == integrationId)
                .OrderBy(x => x.SourceFieldName).Select(x => new MappingDto(x.Id, x.TenantIntegrationConfigurationId, x.SourceFieldName, x.SourceFieldDescription, x.DestinationFieldName)).ToListAsync(token));
        });

        group.MapPost("/tenants/{tenantId:guid}/integrations/{integrationId:guid}/mappings", async (Guid tenantId, Guid integrationId, SaveMappingRequest body, ClaimsPrincipal user, BuilderDbContext db, CancellationToken token) =>
        {
            if (!TryResolveTenantId(user, tenantId, out var currentTenantId)) return Results.Forbid();
            tenantId = currentTenantId;
            if (string.IsNullOrWhiteSpace(body.SourceFieldName) || string.IsNullOrWhiteSpace(body.DestinationFieldName)) return Results.BadRequest("Source and destination field names are required.");
            if (!await db.TenantIntegrationConfigurations.AnyAsync(x => x.Id == integrationId && x.TenantId == tenantId, token)) return Results.NotFound("Integration not found.");
            var entity = new TenantTableMapping { TenantId = tenantId, TenantIntegrationConfigurationId = integrationId,
                SourceFieldName = body.SourceFieldName.Trim(), SourceFieldDescription = body.SourceFieldDescription?.Trim(), DestinationFieldName = body.DestinationFieldName.Trim() };
            db.TenantTableMappings.Add(entity);
            await db.SaveChangesAsync(token);
            return Results.Created($"/builder/tenant-configurations/tenants/{tenantId}/integrations/{integrationId}/mappings/{entity.Id}", new MappingDto(entity.Id, integrationId, entity.SourceFieldName, entity.SourceFieldDescription, entity.DestinationFieldName));
        });

        group.MapPut("/tenants/{tenantId:guid}/integrations/{integrationId:guid}/mappings/{mappingId:guid}", async (Guid tenantId, Guid integrationId, Guid mappingId, SaveMappingRequest body, ClaimsPrincipal user, BuilderDbContext db, CancellationToken token) =>
        {
            if (!TryResolveTenantId(user, tenantId, out var currentTenantId)) return Results.Forbid();
            tenantId = currentTenantId;
            if (string.IsNullOrWhiteSpace(body.SourceFieldName) || string.IsNullOrWhiteSpace(body.DestinationFieldName)) return Results.BadRequest("Source and destination field names are required.");
            var entity = await db.TenantTableMappings.SingleOrDefaultAsync(x => x.Id == mappingId && x.TenantId == tenantId && x.TenantIntegrationConfigurationId == integrationId, token);
            if (entity is null) return Results.NotFound();
            entity.SourceFieldName = body.SourceFieldName.Trim(); entity.SourceFieldDescription = body.SourceFieldDescription?.Trim(); entity.DestinationFieldName = body.DestinationFieldName.Trim(); entity.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(token);
            return Results.Ok(new MappingDto(entity.Id, integrationId, entity.SourceFieldName, entity.SourceFieldDescription, entity.DestinationFieldName));
        });

        group.MapDelete("/tenants/{tenantId:guid}/integrations/{integrationId:guid}/mappings/{mappingId:guid}", async (Guid tenantId, Guid integrationId, Guid mappingId, ClaimsPrincipal user, BuilderDbContext db, CancellationToken token) =>
        {
            if (!TryResolveTenantId(user, tenantId, out var currentTenantId)) return Results.Forbid();
            tenantId = currentTenantId;
            var entity = await db.TenantTableMappings.SingleOrDefaultAsync(x => x.Id == mappingId && x.TenantId == tenantId && x.TenantIntegrationConfigurationId == integrationId, token);
            if (entity is null) return Results.NotFound();
            db.TenantTableMappings.Remove(entity);
            await db.SaveChangesAsync(token);
            return Results.NoContent();
        });
    }

    private static bool IsAdmin(ClaimsPrincipal user) => user.IsInRole(BuilderRoleNames.SystemAdmin) || user.IsInRole(BuilderRoleNames.TenantAdmin);
    private static bool TryTenant(ClaimsPrincipal user, out Guid tenantId)
    {
        foreach (var claimType in new[] { "tenant_id", "tenantId", "company_id", "companyId" })
            if (Guid.TryParse(user.FindFirstValue(claimType), out tenantId) && tenantId != Guid.Empty)
                return true;

        tenantId = Guid.Empty;
        return false;
    }
    private static bool TryResolveTenantId(ClaimsPrincipal user, Guid requestedTenantId, out Guid tenantId)
    {
        if (user.IsInRole(BuilderRoleNames.SystemAdmin))
        {
            tenantId = requestedTenantId;
            return true;
        }

        if (user.IsInRole(BuilderRoleNames.TenantAdmin) && TryTenant(user, out var currentTenantId) && currentTenantId == requestedTenantId)
        {
            tenantId = currentTenantId;
            return true;
        }

        tenantId = Guid.Empty;
        return false;
    }
    private static Task<bool> TenantIsActive(BuilderDbContext db, Guid tenantId, CancellationToken token) =>
        db.Tenants.AnyAsync(x => x.TenantId == tenantId && x.RecordState != RecordState.Deleted, token);

    private static TenantSmtpDto ToSmtpDto(TenantSmtpConfiguration x) => new(x.TenantId, x.Host, x.Port, x.UserName, x.From, x.DisplayName, x.EnableSsl, x.UseStartTls, !string.IsNullOrEmpty(x.Password));
    private static IntegrationDto ToIntegrationDto(TenantIntegrationConfiguration x) => new(x.Id, x.TenantId, x.ApplicationId, x.Name, x.IntegrationType, x.Port, x.UserName, x.DatabaseName, x.EnableSsl, !string.IsNullOrEmpty(x.Password), !string.IsNullOrEmpty(x.Token), !string.IsNullOrEmpty(x.ConnectionString), x.TenantTableMappings.Count);

    private static async Task<string?> ValidateIntegration(SaveIntegrationRequest body, Guid tenantId, BuilderDbContext db, CancellationToken token, Guid? currentId)
    {
        if (string.IsNullOrWhiteSpace(body.Name) || body.Name.Trim().Length > 160 || body.ApplicationId == Guid.Empty || body.Port is < 1 or > 65535)
            return "Name, application, and a valid port are required.";
        if (!Enum.IsDefined(body.IntegrationType)) return "Integration type is invalid.";
        if (!await db.Applications.IgnoreQueryFilters().AnyAsync(x => x.Id == body.ApplicationId && x.TenantId == tenantId, token))
            return "The selected application does not belong to this tenant.";
        var duplicate = await db.TenantIntegrationConfigurations.AnyAsync(x => x.TenantId == tenantId && x.ApplicationId == body.ApplicationId && x.Name == body.Name.Trim() && x.Id != currentId, token);
        if (duplicate) return "An integration with this name already exists for the application.";
        return null;
    }

    private static void ApplyIntegration(TenantIntegrationConfiguration entity, SaveIntegrationRequest body)
    {
        entity.Name = body.Name.Trim(); entity.Port = body.Port; entity.UserName = body.UserName?.Trim(); entity.DatabaseName = body.DatabaseName?.Trim();
        entity.EnableSsl = body.EnableSsl; entity.ApplicationId = body.ApplicationId; entity.IntegrationType = body.IntegrationType;
        if (body.ClearPassword) entity.Password = null; else if (!string.IsNullOrWhiteSpace(body.Password)) entity.Password = body.Password;
        if (body.ClearToken) entity.Token = null; else if (!string.IsNullOrWhiteSpace(body.Token)) entity.Token = body.Token;
        if (body.ClearConnectionString) entity.ConnectionString = null; else if (!string.IsNullOrWhiteSpace(body.ConnectionString)) entity.ConnectionString = body.ConnectionString;
    }
}

public sealed record TenantDto(Guid TenantId, string Name, RecordState RecordState);
public sealed record TenantSmtpDto(Guid TenantId, string Host, int? Port, string? UserName, string From, string? DisplayName, bool? EnableSsl, bool? UseStartTls, bool HasPassword);
public sealed record SaveSmtpRequest(string Host, int? Port, string? UserName, string From, string? DisplayName, string? Password, bool? EnableSsl, bool? UseStartTls, bool ClearPassword = false);
public sealed record IntegrationDto(Guid Id, Guid TenantId, Guid ApplicationId, string Name, IntegrationType IntegrationType, int? Port, string? UserName, string? DatabaseName, bool? EnableSsl, bool HasPassword, bool HasToken, bool HasConnectionString, int MappingCount);
public sealed record SaveIntegrationRequest(string Name, Guid ApplicationId, IntegrationType IntegrationType, int? Port, string? UserName, string? Password, string? Token, string? ConnectionString, string? DatabaseName, bool? EnableSsl, bool ClearPassword = false, bool ClearToken = false, bool ClearConnectionString = false);
public sealed record MappingDto(Guid Id, Guid TenantIntegrationConfigurationId, string SourceFieldName, string? SourceFieldDescription, string DestinationFieldName);
public sealed record SaveMappingRequest(string SourceFieldName, string? SourceFieldDescription, string DestinationFieldName);

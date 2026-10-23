using System.Text.Json;
using System.Text.RegularExpressions;
using SorteFederation.Builders.Data;
using SorteFederation.Builders.Data.Domain;
using SorteFederation.Shared.EndPoints;
using Microsoft.EntityFrameworkCore;

namespace SorteFederation.Builders.Endpoints;

public sealed record SaveEmailTemplateRequest(Guid TenantId, string Key, string Name, string Subject, string Language, string TemplateType, string DefinitionJson);

public sealed class EmailTemplatesEndpoints : IAppEndPoints
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/applications/{applicationId:guid}/email-templates")
            .RequireAuthorization().WithTags("Email templates");

        group.MapGet("", async (Guid applicationId, Guid tenantId, BuilderDbContext db, CancellationToken token) =>
            Results.Ok(await db.EmailTemplates.AsNoTracking()
                .Where(x => x.ApplicationId == applicationId && x.TenantId == tenantId)
                .OrderByDescending(x => x.UpdatedAt).Take(200).ToListAsync(token)));

        group.MapPost("", async (Guid applicationId, SaveEmailTemplateRequest request, BuilderDbContext db, CancellationToken token) =>
        {
            var error = Validate(request);
            if (error is not null) return Results.BadRequest(error);
            if (!await db.Applications.AnyAsync(x => x.Id == applicationId && x.TenantId == request.TenantId, token)) return Results.NotFound();
            if (await db.EmailTemplates.AnyAsync(x => x.ApplicationId == applicationId && x.TenantId == request.TenantId && x.Key == request.Key, token))
                return Results.Conflict("An email template with this key already exists.");
            var entity = new BuilderEmailTemplate { ApplicationId = applicationId, TenantId = request.TenantId,
                Key = request.Key.Trim(), Name = request.Name.Trim(), Subject = request.Subject.Trim(),
                Language = request.Language.Trim(), TemplateType = request.TemplateType.Trim(), DefinitionJson = request.DefinitionJson };
            db.EmailTemplates.Add(entity);
            await db.SaveChangesAsync(token);
            return Results.Created($"/applications/{applicationId}/email-templates/{entity.Id}", entity);
        });

        group.MapPut("/{templateId:guid}", async (Guid applicationId, Guid templateId, SaveEmailTemplateRequest request, BuilderDbContext db, CancellationToken token) =>
        {
            var error = Validate(request);
            if (error is not null) return Results.BadRequest(error);
            var entity = await db.EmailTemplates.SingleOrDefaultAsync(x => x.Id == templateId && x.ApplicationId == applicationId && x.TenantId == request.TenantId, token);
            if (entity is null) return Results.NotFound();
            if (await db.EmailTemplates.AnyAsync(x => x.Id != templateId && x.ApplicationId == applicationId && x.TenantId == request.TenantId && x.Key == request.Key, token))
                return Results.Conflict("An email template with this key already exists.");
            entity.Key = request.Key.Trim(); entity.Name = request.Name.Trim(); entity.Subject = request.Subject.Trim();
            entity.Language = request.Language.Trim(); entity.TemplateType = request.TemplateType.Trim(); entity.DefinitionJson = request.DefinitionJson;
            entity.UpdatedAt = DateTimeOffset.UtcNow; entity.Version++;
            await db.SaveChangesAsync(token);
            return Results.Ok(entity);
        });

        group.MapDelete("/{templateId:guid}", async (Guid applicationId, Guid templateId, Guid tenantId, BuilderDbContext db, CancellationToken token) =>
        {
            var entity = await db.EmailTemplates.SingleOrDefaultAsync(x => x.Id == templateId && x.ApplicationId == applicationId && x.TenantId == tenantId, token);
            if (entity is null) return Results.NotFound();
            db.EmailTemplates.Remove(entity);
            await db.SaveChangesAsync(token);
            return Results.NoContent();
        });
    }

    private static string? Validate(SaveEmailTemplateRequest request)
    {
        if (request.TenantId == Guid.Empty || !Regex.IsMatch(request.Key ?? "", "^[a-z][a-z0-9-]{0,79}$") ||
            string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 120 ||
            string.IsNullOrWhiteSpace(request.Subject) || request.Subject.Length > 250 ||
            string.IsNullOrWhiteSpace(request.Language) || request.Language.Length > 16 ||
            string.IsNullOrWhiteSpace(request.TemplateType) || request.TemplateType.Length > 60 ||
            request.DefinitionJson is null || request.DefinitionJson.Length > 100_000)
            return "Invalid email template metadata.";
        try
        {
            using var document = JsonDocument.Parse(request.DefinitionJson);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("schemaVersion", out var version) || version.GetInt32() != 1 ||
                !root.TryGetProperty("blocks", out var blocks) || blocks.ValueKind != JsonValueKind.Array || blocks.GetArrayLength() > 100)
                return "Unsupported email definition.";
            var paragraphs = 0;
            foreach (var block in blocks.EnumerateArray())
            {
                if (block.ValueKind != JsonValueKind.Object || !block.TryGetProperty("type", out var type) ||
                    type.ValueKind != JsonValueKind.String || type.GetString() is not ("heading" or "paragraph" or "button" or "divider") ||
                    !block.TryGetProperty("text", out var text) || text.ValueKind != JsonValueKind.String || text.GetString()!.Length > 10000)
                    return "Unsupported email block.";
                if (type.GetString() == "paragraph" && ++paragraphs > 1) return "An email template can have only one paragraph body.";
                if (block.TryGetProperty("richText", out var richText))
                {
                    if (type.GetString() != "paragraph" || richText.ValueKind != JsonValueKind.String ||
                        richText.GetString() is not { } delta || !ValidParagraphDelta(delta))
                        return "Unsupported rich-text paragraph.";
                }
                if (type.GetString() == "button" && block.TryGetProperty("url", out var url))
                {
                    if (url.ValueKind != JsonValueKind.String) return "Button links must be text.";
                    if (url.GetString() is { Length: > 0 } link &&
                        (!Uri.TryCreate(link, UriKind.Absolute, out var parsed) || parsed.Scheme is not ("http" or "https")))
                        return "Button links must use HTTP or HTTPS.";
                }
            }
            if (paragraphs != 1) return "An email template requires one paragraph body.";
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException or FormatException or ArgumentException)
        {
            return "Invalid email definition JSON.";
        }
        return null;
    }

    private static bool ValidParagraphDelta(string delta)
    {
        if (delta.Length > 20_000) return false;
        try
        {
            using var document = JsonDocument.Parse(delta);
            if (document.RootElement.ValueKind != JsonValueKind.Object ||
                !document.RootElement.TryGetProperty("ops", out var ops) || ops.ValueKind != JsonValueKind.Array || ops.GetArrayLength() > 200)
                return false;
            var length = 0;
            foreach (var op in ops.EnumerateArray())
            {
                if (op.ValueKind != JsonValueKind.Object || !op.TryGetProperty("insert", out var insert) || insert.ValueKind != JsonValueKind.String)
                    return false; // No image, video, script or other embeds in email drafts.
                length += insert.GetString()!.Length;
                if (length > 10_000) return false;
                if (!op.TryGetProperty("attributes", out var attributes)) continue;
                if (attributes.ValueKind != JsonValueKind.Object) return false;
                foreach (var attribute in attributes.EnumerateObject())
                {
                    if (attribute.Name is "bold" or "italic" or "underline" or "strike" or "blockquote" or "code-block")
                    {
                        if (attribute.Value.ValueKind != JsonValueKind.True) return false;
                    }
                    else if (attribute.Name == "list")
                    {
                        if (attribute.Value.ValueKind != JsonValueKind.String || attribute.Value.GetString() is not ("ordered" or "bullet")) return false;
                    }
                    else if (attribute.Name == "header")
                    {
                        if (attribute.Value.ValueKind != JsonValueKind.Number || !attribute.Value.TryGetInt32(out var header) || header is not (1 or 2)) return false;
                    }
                    else if (attribute.Name == "script")
                    {
                        if (attribute.Value.ValueKind != JsonValueKind.String || attribute.Value.GetString() is not ("sub" or "super")) return false;
                    }
                    else if (attribute.Name == "indent")
                    {
                        if (attribute.Value.ValueKind != JsonValueKind.Number || !attribute.Value.TryGetInt32(out var indent) || indent is < 1 or > 8) return false;
                    }
                    else if (attribute.Name == "direction")
                    {
                        if (attribute.Value.ValueKind != JsonValueKind.String || attribute.Value.GetString() != "rtl") return false;
                    }
                    else if (attribute.Name == "size")
                    {
                        if (attribute.Value.ValueKind != JsonValueKind.String || attribute.Value.GetString() is not ("small" or "large" or "huge")) return false;
                    }
                    else if (attribute.Name is "color" or "background")
                    {
                        if (attribute.Value.ValueKind != JsonValueKind.String || !Regex.IsMatch(attribute.Value.GetString()!, "^#[0-9a-fA-F]{3,8}$")) return false;
                    }
                    else if (attribute.Name == "font")
                    {
                        if (attribute.Value.ValueKind != JsonValueKind.String || attribute.Value.GetString() is not ("serif" or "monospace")) return false;
                    }
                    else if (attribute.Name == "align")
                    {
                        if (attribute.Value.ValueKind != JsonValueKind.String || attribute.Value.GetString() is not ("center" or "right" or "justify")) return false;
                    }
                    else if (attribute.Name == "link")
                    {
                        if (attribute.Value.ValueKind != JsonValueKind.String ||
                            !Uri.TryCreate(attribute.Value.GetString(), UriKind.Absolute, out var link) || link.Scheme is not ("http" or "https")) return false;
                    }
                    else return false;
                }
            }
            return true;
        }
        catch (JsonException) { return false; }
    }
}

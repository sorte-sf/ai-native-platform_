using System.Text.Json;
namespace SorteFederation.Portal.EndPoints;
internal static class EntityEndpointBuilder
{
    public static void MapCrud(IEndpointRouteBuilder group, Type type, string route, string tag)
    {
        group.MapGet(route, (ANTSoftPortalDbContext db) => Results.Ok(Query(db, type))).WithTags(tag);
        group.MapGet(route + "/{id:guid}", async (Guid id, ANTSoftPortalDbContext db, CancellationToken token) =>
        {
            var value = await db.FindAsync(type, new object?[] { id }, token);
            return value is null ? Results.NotFound() : Results.Ok(value);
        }).WithTags(tag);

        group.MapPost(route, async (JsonElement body, ANTSoftPortalDbContext db, CancellationToken token) =>
        {
            var value = body.Deserialize(type, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            if (value is null) return Results.BadRequest();
            db.Add(value);
            await db.SaveChangesAsync(token);
            return Results.Created($"{route}/{type.GetProperty(nameof(BaseEntity.Id)).GetValue(value)}", value);
        }).WithTags(tag);

        group.MapPut(route + "/{id:guid}", async (Guid id, JsonElement body, ANTSoftPortalDbContext db, CancellationToken token) =>
        {
            var current = await db.FindAsync(type, new object?[] { id }, token);
            var replacement = body.Deserialize(type, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            if (current is null || replacement is null) return Results.NotFound();
            db.Entry(current).CurrentValues.SetValues(replacement);
            await db.SaveChangesAsync(token);
            return Results.Ok(current);
        }).WithTags(tag);

        group.MapDelete(route + "/{id:guid}", async (Guid id, ANTSoftPortalDbContext db, CancellationToken token) =>
        {
            var value = await db.FindAsync(type, new object?[] { id }, token);
            if (value is null) return Results.NotFound();
            db.Remove(value);
            await db.SaveChangesAsync(token);
            return Results.NoContent();
        }).WithTags(tag);

    }
    private static List<object> Query(ANTSoftPortalDbContext db, Type type)
    {
        var set = (IQueryable)typeof(DbContext).GetMethod(nameof(DbContext.Set), Type.EmptyTypes)!.MakeGenericMethod(type).Invoke(db, null)!;
        return set.Cast<object>().ToList();
    }
}

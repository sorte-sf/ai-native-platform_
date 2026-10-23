namespace SorteFederation.Builders.Services;

public static class RelationshipCardinalityRules
{
    public static string Normalize(string? value)
    {
        var cardinality = value?.Trim().ToLowerInvariant();
        return cardinality is "one-to-one" or "one-to-many" or "many-to-one" or "many-to-many"
            ? cardinality
            : throw new ArgumentException("Cardinality must be one-to-one, one-to-many, many-to-one, or many-to-many.");
    }

    public static bool SourceIsUnique(string cardinality) =>
        cardinality is "one-to-one" or "many-to-one";

    public static bool TargetIsUnique(string cardinality) =>
        cardinality is "one-to-one" or "one-to-many";
}

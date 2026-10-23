namespace SorteFederation.Builders;

public class ServerUUID
{
    private const string ProductUuidPath = "/sys/class/dmi/id/product_uuid";
    private static readonly Lazy<string> CachedUuid = new(ReadProductUuid, LazyThreadSafetyMode.ExecutionAndPublication);

    public static string Get()
    {
        return CachedUuid.Value;
    }

    private static string ReadProductUuid()
    {
        try
        {
            if (!File.Exists(ProductUuidPath))
            {
                return "unknown";
            }

            var uuid = File.ReadAllText(ProductUuidPath).Trim();
            return string.IsNullOrWhiteSpace(uuid) ? "unknown" : uuid;
        }
        catch
        {
            return "unknown";
        }
    }
}

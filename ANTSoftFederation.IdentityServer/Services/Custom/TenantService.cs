namespace ANTSoftFederation.IdentityServer.Services.Custom;

public static class Tenants
{
    public const string Internet = nameof(Internet);
    public const string TenantId = nameof(TenantId);

    public static IReadOnlyCollection<string> All = new[] { Internet, TenantId };

    public static string Find(string value)
    {
        return All.FirstOrDefault(t => t.Equals(value?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? Internet;
    }

}

public class TenantService : ITenantGetter, ITenantSetter
{
    public string Tenant { get; private set; } = Tenants.Internet;

    public void SetTenant(string tenant)
    {
        Tenant = tenant;
    }
}

public interface ITenantGetter
{
    string Tenant { get; }
}

public interface ITenantSetter
{
    void SetTenant(string tenant);
}

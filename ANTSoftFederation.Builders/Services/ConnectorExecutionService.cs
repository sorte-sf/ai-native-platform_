using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SorteFederation.Builders.Services;

public sealed class ConnectorExecutionService(BuilderDbContext db, IHttpClientFactory clients, ISecretResolver secrets, AuditService audit, IHttpContextAccessor accessor, BuilderMetadataService authorization, IConfiguration configuration)
{
    private const long MaxResponseBytes = 4 * 1024 * 1024;
    private readonly HashSet<string> _allowedHosts = (configuration.GetSection("Builders:Connectors:AllowedHosts").Get<string[]>() ?? [])
        .Where(host => !string.IsNullOrWhiteSpace(host))
        .Select(host => host.Trim().TrimEnd('.'))
        .ToHashSet(StringComparer.OrdinalIgnoreCase);
    public async Task<IResult> Execute(ExecuteConnectorRequest request, CancellationToken token)
    {
        var connector = await db.Connectors.FindAsync([request.ConnectorId], token);

        if (connector is null || connector.TenantId != request.TenantId) return Results.NotFound();

        var user = accessor.HttpContext?.User ?? new System.Security.Claims.ClaimsPrincipal();
        var applicationId = connector.ApplicationId;
        if (!await authorization.HasPermission(request.TenantId, applicationId, "connector", "execute", user, token))
            return Results.Forbid();

        if (!connector.Enabled) return Results.Problem("Connector is disabled.", statusCode: StatusCodes.Status409Conflict);

        var supportedTypes = new[] { "http", "rest", "webhook", "erp", "crm", "hr" };
        if (!supportedTypes.Contains(connector.Type.Trim(), StringComparer.OrdinalIgnoreCase))
            return Results.BadRequest($"Connector type '{connector.Type}' is not supported by the installed connector adapters.");

        using var config = JsonDocument.Parse(connector.ConfigurationJson);

        if (!config.RootElement.TryGetProperty("url", out var urlProperty) || !Uri.TryCreate(urlProperty.GetString(), UriKind.Absolute, out var url)) 
            return Results.BadRequest("Connector configuration requires a valid absolute url.");

        if (!string.Equals(url.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)) return Results.BadRequest("Only HTTPS connector URLs are allowed.");
        if (!_allowedHosts.Contains(url.IdnHost.TrimEnd('.'))) return Results.BadRequest("Connector host is not present in Builders:Connectors:AllowedHosts.");
        if (!await IsSafeDestination(url, token)) return Results.BadRequest("Connector URL resolves to a local or private network address, which is not allowed.");

        var method = config.RootElement.TryGetProperty("method", out var methodProperty) ? methodProperty.GetString() : "POST";
        var methodName = string.IsNullOrWhiteSpace(method) ? "POST" : method.Trim().ToUpperInvariant();
        if (methodName is "CONNECT" or "TRACE" or "OPTIONS")
            return Results.BadRequest("Connector method is invalid or not allowed.");
        using var message = new HttpRequestMessage(new HttpMethod(methodName), url);
        var credential = await db.ConnectorCredentials.AsNoTracking().SingleOrDefaultAsync(x => x.ConnectorId == connector.Id && x.TenantId == request.TenantId && x.Active, token);
        if (credential is not null)
        {
            var secret = secrets.Resolve(credential.SecretReference);
            if (string.Equals(credential.Scheme, "bearer", StringComparison.OrdinalIgnoreCase)) message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", secret);
            else if (string.Equals(credential.Scheme, "api-key", StringComparison.OrdinalIgnoreCase)) message.Headers.TryAddWithoutValidation("X-API-Key", secret);
            else throw new InvalidOperationException($"Unsupported connector credential scheme '{credential.Scheme}'.");
        }

        if (!string.IsNullOrWhiteSpace(request.PayloadJson)) message.Content = JsonContent.Create(JsonSerializer.Deserialize<JsonElement>(request.PayloadJson));
        HttpResponseMessage? response = null;
        Exception? lastError = null;
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                response = await clients.CreateClient("builders-connectors").SendAsync(message, token);
                if ((int)response.StatusCode < 500 || attempt == 3) break;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                lastError = ex;
                if (attempt == 3) break;
            }
            await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), token);
        }
        if (response is null)
        {
            await audit.Record(new CreateAuditEventRequest(request.TenantId, "connector.failed", "connector", connector.Id,
                JsonSerializer.Serialize(new { method = methodName, error = lastError?.GetType().Name ?? "NoResponse" })),
                user, accessor.HttpContext?.TraceIdentifier, token);
            throw new HttpRequestException("Connector execution failed after three attempts.", lastError);
        }

        if (response.Content.Headers.ContentLength is > MaxResponseBytes)
            return Results.Problem("Connector response exceeds the 4 MB limit.", statusCode: StatusCodes.Status502BadGateway);
        await response.Content.LoadIntoBufferAsync((int)MaxResponseBytes, token);
        var body = await response.Content.ReadAsStringAsync(token);
        if (Encoding.UTF8.GetByteCount(body) > MaxResponseBytes)
            return Results.Problem("Connector response exceeds the 4 MB limit.", statusCode: StatusCodes.Status502BadGateway);
        await audit.Record(new CreateAuditEventRequest(request.TenantId, "connector.executed", "connector", connector.Id,
            JsonSerializer.Serialize(new { method = methodName, statusCode = (int)response.StatusCode })),
            user, accessor.HttpContext?.TraceIdentifier, token);
        return Results.Content(body, response.Content.Headers.ContentType?.ToString() ?? "application/json", statusCode: (int)response.StatusCode);
    }

    private static async Task<bool> IsSafeDestination(Uri url, CancellationToken token)
    {
        IPAddress[] addresses;
        if (IPAddress.TryParse(url.Host, out var literal)) addresses = [literal];
        else try { addresses = await Dns.GetHostAddressesAsync(url.IdnHost, token); }
        catch (SocketException) { return false; }
        return addresses.Length > 0 && addresses.All(IsPublicAddress);
    }

    private static bool IsPublicAddress(IPAddress address)
    {
        if (IPAddress.IsLoopback(address) || address.IsIPv4MappedToIPv6 || address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any) || address.Equals(IPAddress.IPv6Loopback)) return false;
        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            var bytes = address.GetAddressBytes();
            return !(bytes[0] == 0 || bytes[0] == 10 || bytes[0] == 127 ||
                (bytes[0] == 100 && bytes[1] is >= 64 and <= 127) ||
                (bytes[0] == 169 && bytes[1] == 254) ||
                (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
                (bytes[0] == 192 && (bytes[1] == 0 || bytes[1] == 168)) ||
                (bytes[0] == 198 && bytes[1] is 18 or 19) || bytes[0] >= 224);
        }
        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            var bytes = address.GetAddressBytes();
            var documentationRange = bytes[0] == 0x20 && bytes[1] == 0x01 && bytes[2] == 0x0d && bytes[3] == 0xb8;
            var uniqueLocalRange = (bytes[0] & 0xfe) == 0xfc;
            var siteLocalRange = bytes[0] == 0xfe && (bytes[1] & 0xc0) == 0xc0;
            var teredoRange = bytes[0] == 0x20 && bytes[1] == 0x01 && bytes[2] == 0 && bytes[3] == 0;
            var sixToFourRange = bytes[0] == 0x20 && bytes[1] == 0x02;
            return !address.IsIPv6LinkLocal && !address.IsIPv6Multicast &&
                !siteLocalRange && !teredoRange && !sixToFourRange && !uniqueLocalRange && !documentationRange;
        }
        return false;
    }

}

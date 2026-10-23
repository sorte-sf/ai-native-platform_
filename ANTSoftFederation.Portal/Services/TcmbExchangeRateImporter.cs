using System.Globalization;
using System.Xml.Linq;

namespace SorteFederation.ANTApplications.Services;

public sealed class TcmbExchangeRateImporter(HttpClient httpClient, IServiceScopeFactory scopeFactory)
{
    public async Task<int> ImportAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var (xml, publicationDate) = await DownloadLatestAvailableAsync(date, cancellationToken);
        var document = XDocument.Parse(xml);
        var sourceDate = ParseSourceDate(document.Root?.Attribute("Date")?.Value, publicationDate);
        var rateDate = sourceDate;

        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ANTSoftPortalDbContext>();
        var imported = 0;
        var parsedRates = new Dictionary<string, TcmbRate>(StringComparer.OrdinalIgnoreCase);

        var allowedCurrencyCodeList = await db.CurrencyTypes
            .Select(x => x.Code.Trim().ToUpper())
            .Distinct()
            .ToListAsync(cancellationToken);

        var allowedCurrencyCodes = allowedCurrencyCodeList.ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (allowedCurrencyCodes.Count == 0)
        {
            return 0;
        }

        foreach (var item in document.Root?.Elements("Currency") ?? [])
        {
            var code = item.Attribute("CurrencyCode")?.Value?.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(code)) continue;
            if (!allowedCurrencyCodes.Contains(code)) continue;

            var name = Value(item, "Isim") ?? Value(item, "CurrencyName") ?? code;
            var unit = int.TryParse(Value(item, "Unit"), out var parsedUnit) ? parsedUnit : 1;
            var rate = await db.DailyExchangeRates.SingleOrDefaultAsync(x => x.RateDate == rateDate && x.CurrencyCode == code, cancellationToken);
            if (rate is null)
            {
                rate = new DailyExchangeRate(rateDate, code, name, unit);
                db.DailyExchangeRates.Add(rate);
                imported++;
            }

            rate.CurrencyName = name;
            rate.Unit = unit;
            rate.ForexBuying = Decimal(item, "ForexBuying");
            rate.ForexSelling = Decimal(item, "ForexSelling");
            rate.BanknoteBuying = Decimal(item, "BanknoteBuying");
            rate.BanknoteSelling = Decimal(item, "BanknoteSelling");

            parsedRates[code] = new TcmbRate(
                code,
                unit,
                rate.ForexBuying,
                rate.ForexSelling,
                rate.BanknoteBuying,
                rate.BanknoteSelling);
        }

        await db.SaveChangesAsync(cancellationToken);
        return imported;
    }

    private async Task<(string Xml, DateOnly PublicationDate)> DownloadLatestAvailableAsync(DateOnly requestedDate, CancellationToken cancellationToken)
    {
        for (var daysBack = 0; daysBack <= 10; daysBack++)
        {
            var candidate = requestedDate.AddDays(-daysBack);
            var url = $"https://www.tcmb.gov.tr/kurlar/{candidate:yyyyMM}/{candidate:ddMMyyyy}.xml";
            using var response = await httpClient.GetAsync(url, cancellationToken);
            if (response.IsSuccessStatusCode)
                return (await response.Content.ReadAsStringAsync(cancellationToken), candidate);
            if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
                response.EnsureSuccessStatusCode();
        }

        throw new HttpRequestException($"TCMB did not publish an exchange-rate file on or within 10 days before {requestedDate:yyyy-MM-dd}.");
    }

    private static string? Value(XElement item, string name) => item.Element(name)?.Value?.Trim();

    private static decimal? Decimal(XElement item, string name) => decimal.TryParse(Value(item, name), NumberStyles.Number, CultureInfo.InvariantCulture, out var value) ? value : null;

    private static DateOnly ParseSourceDate(string? value, DateOnly fallback) => DateOnly.TryParseExact(value, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : fallback;

    private static decimal NormalizeRate(decimal? value, int unit) => value.GetValueOrDefault() / Math.Max(unit, 1);

    private static decimal ApplyExchangeDiscount(decimal value, decimal discount) => Math.Round(value * (1 - discount / 100), 6, MidpointRounding.AwayFromZero);

    private sealed record TcmbRate(
        string Code,
        int Unit,
        decimal? ForexBuying,
        decimal? ForexSelling,
        decimal? BanknoteBuying,
        decimal? BanknoteSelling,
        Guid? CountryId = null
        );
}

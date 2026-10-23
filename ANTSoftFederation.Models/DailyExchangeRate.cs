using System.ComponentModel.DataAnnotations;

namespace SorteFederation.Models;

public sealed class DailyExchangeRate(DateOnly rateDate, string currencyCode, string currencyName, int unit)
{
    public Guid Id { get; set; } = Guid.CreateVersion7(); 
    public DateOnly RateDate { get; set; } = rateDate;
    [MaxLength(10)] public string CurrencyCode { get; set; } = currencyCode;
    [MaxLength(120)] public string CurrencyName { get; set; } = currencyName;
    public int Unit { get; set; } = unit;
    public decimal? ForexBuying { get; set; }
    public decimal? ForexSelling { get; set; }
    public decimal? BanknoteBuying { get; set; }
    public decimal? BanknoteSelling { get; set; }
    public Guid? CountryId { get; set; }
    public Country? Country { get; set; }

}

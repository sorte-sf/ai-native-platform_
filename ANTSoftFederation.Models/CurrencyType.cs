using System.ComponentModel.DataAnnotations;

namespace SorteFederation.Models;

public sealed class CurrencyType(string code, string name, string symbol)
{
    public Guid Id { get; set; } = Guid.CreateVersion7(); 
    [MaxLength(10)] public string Code { get; set; } = code;
    [MaxLength(120)] public string Name { get; set; } = name;
    [MaxLength(10)] public string Symbol { get; set; } = symbol;
    public decimal ExchangeDiscount { get; set; }
}

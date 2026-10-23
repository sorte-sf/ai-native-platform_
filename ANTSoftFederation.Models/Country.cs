using System.ComponentModel.DataAnnotations;

namespace SorteFederation.Models;

public sealed class Country(string code, string name)
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    [MaxLength(10)] public string Code { get; set; } = code;
    [MaxLength(256)] public string Name { get; set; } = name;
    public bool IsActive { get; set; } = true;
}

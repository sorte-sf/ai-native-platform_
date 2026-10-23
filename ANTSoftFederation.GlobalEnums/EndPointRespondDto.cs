namespace SorteFederation.GlobalEnums;

public class EndPointRespondDto
{
    public string? Id { get; set; } = string.Empty;
    public string? MessageCode { get; set; } = string.Empty;
    public string? Message { get; set; } = string.Empty;
    public bool? Success { get; set; } = false;
    public string? Token { get; set; } = string.Empty;
    public string? Type { get; set; } = string.Empty;
    public int? ExpiresInSeconds { get; set; }
}
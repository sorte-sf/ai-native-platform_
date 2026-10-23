
namespace SorteFederation.Logger.Options;

public class ELKLogstashOptions {

    public bool Enabled { get; set; } = false;
    
    public string RequestUri { get; set; }

    public int BatchPostingLimit { get; set; } = 1000;

    public int? QueueLimit { get; set; }

    public TimeSpan? Period { get; set; } 
    
    public string Username { get; set; } 
    
    public string Password { get; set; }
}
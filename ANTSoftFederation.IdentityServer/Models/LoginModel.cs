namespace ANTSoftFederation.IdentityServer.Models;

public record LoginModel(string Username, string Password, string? TwoFactorCode);

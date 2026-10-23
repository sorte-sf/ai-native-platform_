namespace ANTSoftFederation.IdentityServer.Models;

public class LoginRespondDto
{
    public bool Success { get; set; } = false;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public bool RequiresTwoFactor { get; set; } = false;
    public string? MessageCode { get; set; } = null;
    public string? Message { get; set; } = null;
    public int? ExpiresInSeconds { get; set; }
}

public record LoginResponse(string AccessToken, string RefreshToken, int ExpiresIn, string TokenType = "Bearer");

//public record RefreshTokenRequest(string RefreshToken, string? ClientId = null);


/*
public class LoginResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = "Bearer"

    public LoginResponse(string accessToken, string refreshToken, int expiresIn, string tokenType)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiresIn = expiresIn;
        TokenType = tokenType;
    }
}*/

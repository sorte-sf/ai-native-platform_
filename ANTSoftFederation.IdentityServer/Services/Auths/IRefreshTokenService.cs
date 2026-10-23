namespace ANTSoftFederation.IdentityServer.Services.Auths;

public interface IRefreshTokenService
{
    Task<RefreshToken> CreateAsync(Guid userId);
    Task<RefreshToken> GetByTokenAsync(string token);
    Task<bool> RevokeAsync(string token, string replacedByToken = null);
    Task<bool> RevokeAllUserTokensAsync(Guid userId);
}

public class RefreshTokenService : IRefreshTokenService
{
    private readonly ANTSoftIdentityDbContext _context;
    private readonly ITokenService _tokenService;

    public RefreshTokenService(ANTSoftIdentityDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<RefreshToken> CreateAsync(Guid userId)
    {
        var refreshToken = _tokenService.GenerateRefreshToken(userId);
        refreshToken.UserId = userId;

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken;
    }

    public async Task<RefreshToken> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<bool> RevokeAsync(string token, string replacedByToken = null)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken == null || !refreshToken.IsActive)
            return false;

        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        // refreshToken.RevokedBy = ipAddress;
        refreshToken.ReplacedByToken = replacedByToken;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RevokeAllUserTokensAsync(Guid userId)
    {
        var refreshTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.IsActive)
            .ToListAsync();

        foreach (var token in refreshTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
           // token.RevokedBy = ipAddress;
        }

        await _context.SaveChangesAsync();
        return true;
    }
}

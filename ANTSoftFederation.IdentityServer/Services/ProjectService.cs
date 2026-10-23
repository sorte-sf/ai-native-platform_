
namespace ANTSoftFederation.IdentityServer.Services;

public class ProjectService : BaseService, IProjectService
{
    private readonly ANTSoftIdentityDbContext _context;
    private readonly IMemoryCache _cache;

    public ProjectService(ANTSoftIdentityDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<(IEnumerable<ProjectListDto> Projects, int TotalCount)> GetProjectsAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null)
    {
        var query = _context.Projects.AsNoTracking().AsQueryable();
         
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
        }
         
        var totalCount = await query.CountAsync();
         
        var projects = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProjectListDto
            {
                Id = p.Id,
                Name = p.Name,
                DisplayName = p.DisplayName,
                Description = p.Description,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                IsActive = p.RecordState == RecordState.Active
            })
            .ToListAsync();

        return (projects, totalCount);
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(Guid id)
    {
        var cacheKey = $"project_{id}";
        
        if (!_cache.TryGetValue(cacheKey, out ProjectDto? project))
        {
            project = await _context.Projects.AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new ProjectDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    DisplayName = p.DisplayName,
                    Description = p.Description,
                    Logo = p.Logo,
                    SecretKey = p.SecretKey,
                    PublicKey = p.PublicKey,
                    SslRequired = p.SslRequired,
                    LoginWithEmailAllowed = p.LoginWithEmailAllowed,
                    DuplicateEmailsAllowed = p.DuplicateEmailsAllowed,
                    VerifyEmail = p.VerifyEmail,
                    RegistrationAllowed = p.RegistrationAllowed,
                    RegistrationEmailAsUsername = p.RegistrationEmailAsUsername,
                    RememberMe = p.RememberMe,
                    EditUsernameAllowed = p.EditUsernameAllowed,
                    ResetPasswordAllowed = p.ResetPasswordAllowed,
                    AccessTokenLifespan = p.AccessTokenLifespan,
                    RefreshTokenMaxReuse = p.RefreshTokenMaxReuse,
                    SsoSessionIdleTimeout = p.SsoSessionIdleTimeout,
                    SsoSessionMaxLifespan = p.SsoSessionMaxLifespan,
                    OfflineSessionIdleTimeout = p.OfflineSessionIdleTimeout,
                    PasswordPolicy = p.PasswordPolicy,
                    LoginTheme = p.LoginTheme,
                    AccountTheme = p.AccountTheme,
                    AdminTheme = p.AdminTheme,
                    EmailTheme = p.EmailTheme,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    CreatedBy = p.CreatedBy,
                    UpdatedBy = p.UpdatedBy,
                    IsActive = p.RecordState == RecordState.Active
                })
                .FirstOrDefaultAsync();

            if (project != null)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10));

                _cache.Set(cacheKey, project, cacheEntryOptions);
            }
        }

        return project;
    }

    public async Task<string> CreateProjectAsync(CreateProjectDto createDto)
    {
        var appKeys = KeyGenerator.GenerateAppKeys();

        createDto.PublicKey = appKeys.publicKey;
        createDto.SecretKey = appKeys.secretKey;

        var project = new Project
        {
            Name = createDto.Name,
            DisplayName = createDto.DisplayName,
            Description = createDto.Description,
            Logo = createDto.Logo,
            SecretKey = createDto.SecretKey,
            PublicKey = createDto.PublicKey,
            SslRequired = createDto.SslRequired,
            LoginWithEmailAllowed = createDto.LoginWithEmailAllowed,
            DuplicateEmailsAllowed = createDto.DuplicateEmailsAllowed,
            VerifyEmail = createDto.VerifyEmail,
            RegistrationAllowed = createDto.RegistrationAllowed,
            RegistrationEmailAsUsername = createDto.RegistrationEmailAsUsername,
            RememberMe = createDto.RememberMe,
            EditUsernameAllowed = createDto.EditUsernameAllowed,
            ResetPasswordAllowed = createDto.ResetPasswordAllowed,
            AccessTokenLifespan = createDto.AccessTokenLifespan,
            RefreshTokenMaxReuse = createDto.RefreshTokenMaxReuse,
            SsoSessionIdleTimeout = createDto.SsoSessionIdleTimeout,
            SsoSessionMaxLifespan = createDto.SsoSessionMaxLifespan,
            OfflineSessionIdleTimeout = createDto.OfflineSessionIdleTimeout,
            PasswordPolicy = createDto.PasswordPolicy,
            LoginTheme = createDto.LoginTheme,
            AccountTheme = createDto.AccountTheme,
            AdminTheme = createDto.AdminTheme,
            EmailTheme = createDto.EmailTheme,
            CreatedBy = createDto.CreatedBy
        };

        _context.Projects.Add(project);
        var success  = await _context.SaveChangesAsync()>0;

        return success == true ? project.PublicKey :  "error_creating_project";
    }

    public async Task<bool> UpdateProjectAsync(Guid id, UpdateProjectDto updateDto)
    {
        var project = await _context.Projects.FindAsync(id);
        
        if (project == null)
            return false;

        project.Name = updateDto.Name;
        project.DisplayName = updateDto.DisplayName;
        project.Description = updateDto.Description;
        project.Logo = updateDto.Logo; 
        project.SslRequired = updateDto.SslRequired;
        project.LoginWithEmailAllowed = updateDto.LoginWithEmailAllowed;
        project.DuplicateEmailsAllowed = updateDto.DuplicateEmailsAllowed;
        project.VerifyEmail = updateDto.VerifyEmail;
        project.RegistrationAllowed = updateDto.RegistrationAllowed;
        project.RegistrationEmailAsUsername = updateDto.RegistrationEmailAsUsername;
        project.RememberMe = updateDto.RememberMe;
        project.EditUsernameAllowed = updateDto.EditUsernameAllowed;
        project.ResetPasswordAllowed = updateDto.ResetPasswordAllowed;
        project.AccessTokenLifespan = updateDto.AccessTokenLifespan;
        project.RefreshTokenMaxReuse = updateDto.RefreshTokenMaxReuse;
        project.SsoSessionIdleTimeout = updateDto.SsoSessionIdleTimeout;
        project.SsoSessionMaxLifespan = updateDto.SsoSessionMaxLifespan;
        project.OfflineSessionIdleTimeout = updateDto.OfflineSessionIdleTimeout;
        project.PasswordPolicy = updateDto.PasswordPolicy;
        project.LoginTheme = updateDto.LoginTheme;
        project.AccountTheme = updateDto.AccountTheme;
        project.AdminTheme = updateDto.AdminTheme;
        project.EmailTheme = updateDto.EmailTheme;
        project.UpdatedBy = updateDto.UpdatedBy;
        project.UpdatedAt = DateTime.UtcNow;

        var success = await _context.SaveChangesAsync() > 0;
        
        if (success)
        {
            _cache.Remove($"project_{id}");
        }

        return success;
    }

    public async Task<bool> DeleteProjectAsync(Guid id)
    {
        var project = await _context.Projects.FindAsync(id);
        
        if (project == null)
            return false;

        project.RecordState = RecordState.Deleted;
        project.UpdatedAt = DateTime.UtcNow;
        
        var success = await _context.SaveChangesAsync() > 0;
        
        if (success)
        {
            _cache.Remove($"project_{id}");
        }

        return success;
    }

}
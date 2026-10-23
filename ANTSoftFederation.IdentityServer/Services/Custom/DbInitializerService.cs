
namespace ANTSoftFederation.IdentityServer.Services.Custom;

public interface IDbInitializerService
{
    void Initialize();
    void SeedGroupData();
    void SeedUserData();
    void SeedDefaultProject();
    void SeedDefaultPermission();
}

public class DbInitializerService : IDbInitializerService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ISecurityService _securityService;
    public readonly ILogger<DbInitializerService> _logger;

    public DbInitializerService(
           IServiceScopeFactory scopeFactory,
           ILogger<DbInitializerService> logger,
           ISecurityService securityService
        )
    {
        _scopeFactory = scopeFactory;
        _securityService = securityService;
        _logger = logger;
    }

    public void Initialize()
    {
        try
        {
            _logger.LogInformation("Ensure Storify User Management Database");

            using var serviceScope = _scopeFactory.CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<ANTSoftIdentityDbContext>();

            Policy
            .Handle<Exception>()
            .WaitAndRetry(5, r => TimeSpan.FromSeconds(5), (ex, ts) => { _logger.LogError("Error connecting to DB. Retrying in 5 sec."); })
            .Execute(() => context.Database.Migrate());

            _logger.LogInformation("Storify User Management Database available");
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public void SeedGroupData()
    {
        try
        {
            using var serviceScope = _scopeFactory.CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<ANTSoftIdentityDbContext>();

            if (!context.Groups.Any())
            {
                var group = new Group
                {
                    Id = Guid.Parse("83921C22-745E-48A3-94AC-A54E450CBE59"),
                    Name = "SuperAdmin",
                    CreatedAt = DateTime.UtcNow,
                    ParentGroupId = null,
                };

                context.Add(group);
                context.SaveChanges();
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.InnerException.Message);
            throw new InvalidOperationException(ex.InnerException.Message.ToString());
        }
    }

    public void SeedUserData()
    {
        try
        {
            using var serviceScope = _scopeFactory.CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<ANTSoftIdentityDbContext>();

            if (!context.Users.Any())
            {
                var _passwordSalt = _securityService.GetpasswordSalt();

                var userData = new User
                {
                    Id = Guid.Parse("FFC0E632-6440-4FED-B71F-E6C7491B168A"),
                    Username = "system.admin@teknopalas.com.tr",
                    Email = "system.admin@teknopalas.com.tr",
                    Salt = _passwordSalt,
                    EmailVerified = true,
                    Password = _securityService.GetpasswordHash(_passwordSalt, "Abc12345678!"),
                    CreatedAt = DateTime.UtcNow,
                    UserGroups = new List<UserGroup>
                    {
                        new UserGroup
                        {
                            GroupId = Guid.Parse("83921C22-745E-48A3-94AC-A54E450CBE59"),
                            CreatedAt = DateTime.UtcNow,
                            RecordState = RecordState.Active,
                        }
                    }
                };

                context.Add(userData);
                context.SaveChanges();

            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.InnerException.Message);
            throw new InvalidOperationException(ex.InnerException.Message.ToString());
        }
    }

    public void SeedDefaultProject()
    {
        try
        {
            using var serviceScope = _scopeFactory.CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<ANTSoftIdentityDbContext>();
             
            if (!context.Projects.Any())
            {
               var appKeys = KeyGenerator.GenerateAppKeys();

                var newProject = new Project
                {
                    Id = Guid.Parse("B1D7AF97-D8B9-4269-B58F-1E4B23B98780"),
                    Name = "Master Project",
                    DisplayName = "Master Project",
                    Description = "This is the master project for Butterfly Identity Server.",
                    PublicKey = "o1QuCkms3IpQHbFRIJfWeYVCb2djSdTC1QQGgK8V5ESZe1Eiz6H3eNuiHgHZRhWD",
                    SecretKey = "vaGaKHlwlYKwUQcjn2KkL6MjKujJ3o9YxnEw5T1hXYTWgm6gmvT1Qloqw2w9IyF2",
                    CreatedAt = DateTime.UtcNow,
                    Roles = new List<Role>
                    {
                        new Role{
                            Id = Guid.Parse("CE697541-A59F-42CD-995C-A977E39DD0AC"),
                            Name = "SuperAdmin",
                            Composite= false,
                            UserRoles = new List<UserRole>
                            {
                                new UserRole
                                {
                                   UserId = Guid.Parse("FFC0E632-6440-4FED-B71F-E6C7491B168A"),
                                   CreatedAt = DateTime.UtcNow,
                                   RecordState = RecordState.Active,
                                }
                            }
                        }
                    },
                    UserProjects = new List<UserProject>
                    {
                        new UserProject
                        {
                            UserId = Guid.Parse("FFC0E632-6440-4FED-B71F-E6C7491B168A"),
                            CreatedAt = DateTime.UtcNow,
                            RecordState = RecordState.Active,
                        }
                    },
                    ProjectGroups = new List<ProjectGroup>
                    {
                        new ProjectGroup{ GroupId = Guid.Parse("83921C22-745E-48A3-94AC-A54E450CBE59") }
                    }
                };

                context.Add(newProject);
                context.SaveChanges();
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.InnerException.Message);
            throw new InvalidOperationException(ex.InnerException.Message.ToString());
        }
    }

    public void SeedDefaultPermission()
    {
        try
        {
            using var serviceScope = _scopeFactory.CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<ANTSoftIdentityDbContext>();

            if (!context.Permissions.Any())
            {
                var newPermissions = new Permission
                {
                    //Id = Guid.Parse("E5510DCD-A85D-4264-A6A1-A7D41C7CCD1A"),
                    Name = "Master.Create",
                    ProjectId = Guid.Parse("B1D7AF97-D8B9-4269-B58F-1E4B23B98780"),
                    Description = "Master Project Create Permission",
                    IsSystemPermission = true,
                    CreatedAt = DateTime.UtcNow,
                    RolePermissions = new List<RolePermission>
                    {
                        new RolePermission
                        {
                            RoleId = Guid.Parse("CE697541-A59F-42CD-995C-A977E39DD0AC"),
                            CreatedAt = DateTime.UtcNow,
                            RecordState = RecordState.Active,
                        }
                    },
                    UserPermissions = new List<UserPermission>
                    {
                        new UserPermission
                        { 
                            UserId = Guid.Parse("FFC0E632-6440-4FED-B71F-E6C7491B168A"),
                            CreatedAt = DateTime.UtcNow,
                            RecordState = RecordState.Active,
                        }
                    }
                };

                context.Add(newPermissions);
                context.SaveChanges();
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.InnerException.Message);
            throw new InvalidOperationException(ex.InnerException.Message.ToString());
        }
    }

}
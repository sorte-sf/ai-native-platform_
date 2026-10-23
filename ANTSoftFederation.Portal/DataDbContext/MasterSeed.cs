 
using SorteFederation.Shared.Realtime;
using SorteFederation.Shared;

namespace SorteFederation.Portal.DataDbContext;

public static class MasterSeed
{
    private const string UserName = "sysadmin";

    private static readonly string[] ApplicationName = [
        "Sorte Portal Builder"
    ];

    private static readonly string[] PermissionNames = ["Dashboard.View", "Security.Manage"];

    public static async Task SeedAsync(ANTSoftPortalDbContext db,
        ISecurityService securityService,
        IRealtimeEventBus eventBus,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var defaultCompany = await EnsureDefaultCompanyAsync(db);
        try
        {
            await eventBus.PublishAsync(new RealtimeEvent
            {
                // Portal Company.Id is the canonical Builder TenantId.
                TenantId = defaultCompany.Id,
                EventName = RabbitMQTopics.CompanyCreated,
                UserFacing = false,
                Data = System.Text.Json.JsonSerializer.SerializeToElement(new
                {
                    companyId = defaultCompany.Id,
                    name = defaultCompany.Name
                })
            }, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            // MasterSeed runs on every Portal startup, so a later startup retries
            // this idempotent sync without preventing Portal from becoming available.
            logger.LogError(exception,
                "Could not publish the default company {CompanyId} to the Builder tenant directory. The sync will be retried on the next startup.",
                defaultCompany.Id);
        }

        foreach (var appName in ApplicationName)
        {
            var app = await db.ProjectApplications.SingleOrDefaultAsync(x => x.Name == appName);

            if (app is null)
            {
                app = new ProjectApplication
                {
                    Name = appName,
                    DefaultTheme = "default",
                    DefaultLanguage = "en",
                    GlobalFontSize = "medium",
                    ThemeColors = "{}",
                    Descriptions = appName,
                    AppKey = $"api_{Guid.CreateVersion7():N}",
                    RealmKey = "sorte",
                    SecretKey = Guid.CreateVersion7().ToString("N"),
                    DbVersion = "1.0.0",
                    AppLogo = string.Empty,
                    KeySalt = Guid.CreateVersion7().ToString("N"),
                    AllowCrossOrigin = "false",
                    ConnectionString = string.Empty,
                    DatabaseType = DatabaseType.PostgreSQL,
                    CompanyApplications = new List<CompanyApplication> { new CompanyApplication {
                        CompanyId = defaultCompany.Id ,
                        DataBaseType = DatabaseType.PostgreSQL,
                        ConnectionString = string.Empty,
                        ContractDate = DateOnly.FromDateTime(DateTime.UtcNow),
                        InstallationDate = DateOnly.FromDateTime(DateTime.UtcNow),
                        Note = ""
                    } }
                };

                db.ProjectApplications.Add(app);
                await db.SaveChangesAsync();
            }

            var module = await db.ApplicationModules.SingleOrDefaultAsync(x => x.ApplicationId == app.Id && x.Name == "System");
            if (module is null)
            {
                module = new ApplicationModule { Name = "System", Descriptions = "System permissions", ApplicationId = app.Id };
                db.ApplicationModules.Add(module);
                await db.SaveChangesAsync();
            }

            var permissions = new List<Permission>();
            foreach (var name in PermissionNames)
            {
                var permission = await db.Permissions.SingleOrDefaultAsync(x => x.Name == name && x.ApplicationModuleId == module.Id);
                if (permission is null)
                {
                    permission = new Permission { Name = name, Description = name, ApplicationModuleId = module.Id, IsSystemPermission = true };
                    db.Permissions.Add(permission);
                    await db.SaveChangesAsync();
                }
                permissions.Add(permission);
            }

            var superAdmin = await EnsureRoleAsync(db, RoleNames.SystemAdmin, "Full platform access", permissions);
            await EnsureRoleAsync(db, RoleNames.Admin, "Portal administrator", permissions.Where(x => x.Name != "Security.Manage"));
            await EnsureRoleAsync(db, RoleNames.TenantAdmin, "Tenant Admin", permissions.Where(x => x.Name == "Tenant.Admin"));
            await EnsureRoleAsync(db, RoleNames.PortalUser, "Portal user", permissions.Where(x => x.Name == "Portal.View"));
            await EnsureRoleAsync(db, RoleNames.Designer, "Design and manage reusable application templates", []);
            await EnsureRoleAsync(db, RoleNames.Integrator, "Integrator user", permissions.Where(x => x.Name == "Portal.View"));
            await EnsureRoleAsync(db, RoleNames.User, "user", permissions.Where(x => x.Name == "Portal.View"));

            var user = await db.Users.Include(x => x.UserRoles).SingleOrDefaultAsync(x => x.Username == UserName);
            if (user is null)
            {
                var salt = securityService.GetpasswordSalt();
                var pwrd = securityService.GetpasswordHash(salt, "ChangeMe123!");

                user = new User
                {
                    Id = Guid.CreateVersion7(),
                    Username = UserName,
                    Email = "sysadmin@sorte.local",
                    DisplayName = "System Admin",
                    Password = pwrd,
                    IsSystemAccount = true,
                    EmailVerified = true,
                    Salt = salt,
                    TelePhone = "",
                    CompanyId = defaultCompany.Id,
                    ChangePasswordOnFirstLogin = false,
                    AllowMultipleSessions = true,
                    TwoFactorEnabled = false,
                };
                db.Users.Add(user);
                await db.SaveChangesAsync();
            }
            if (!user.UserRoles.Any(x => x.RoleId == superAdmin.Id))
            {
                db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = superAdmin.Id });
                await db.SaveChangesAsync();
            }

            if (!await db.Countries.AnyAsync(x => x.Code == "TR"))
            {
                db.Countries.Add(new Country("TR", "Turkiye"));
                await db.SaveChangesAsync();
            }
        }
    
    }

    private static async Task<Role> EnsureRoleAsync(ANTSoftPortalDbContext db, string name, string description, IEnumerable<Permission> permissions)
    {
        var role = await db.Roles.SingleOrDefaultAsync(x => x.Name == name);
        if (role is null)
        {
            role = new Role { Name = name, Description = description };
            db.Roles.Add(role);
            await db.SaveChangesAsync();
        }
        var ids = permissions.Select(x => x.Id).ToHashSet();
        var existing = await db.RolePermissions.Where(x => x.RoleId == role.Id && ids.Contains(x.PermissionId)).Select(x => x.PermissionId).ToListAsync();

        foreach (var permission in permissions.Where(x => !existing.Contains(x.Id)))
            db.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });

        await db.SaveChangesAsync();
        return role;
    }

    private static async Task<Company> EnsureDefaultCompanyAsync(ANTSoftPortalDbContext db)
    {
        var checkIfCompanyExists = await db.Companies.IgnoreQueryFilters()
            .SingleOrDefaultAsync(f => f.Name == "Sorte Bilim Ltd Şti");

        if (checkIfCompanyExists != null)
        {
            if (checkIfCompanyExists.RecordState != RecordState.Active)
            {
                checkIfCompanyExists.RecordState = RecordState.Active;
                checkIfCompanyExists.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }
            return checkIfCompanyExists;
        }

        var company = new Company
        {
            Name = "Sorte Bilim Ltd Şti",
            RegistrationNo = "1234567890",
            ContactPersonName = "System Admin",
            ContactPersonEmailAddress = "sysadmin",

            TaxOfficeName = "Tax Office",
            TaxNumber = "1234567890",
            ContactPersonTelephoneNo = "+90 555 555 55 55",
            LogoUrl = "https://www.sorte.com/logo.png",
            Address = "Istanbul - Turkiye"
        };
        db.Companies.Add(company);
        await db.SaveChangesAsync();
        return company;
    }

}

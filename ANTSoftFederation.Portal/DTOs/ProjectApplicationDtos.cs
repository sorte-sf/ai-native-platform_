 
namespace SorteFederation.ANTApplications.DTOs;

public class CreateProjectApplicationDto : BaseCreateDto
{
    public required string Name { get; set; }
    public string DefaultTheme { get; set; } = string.Empty;
    public string DefaultLanguage { get; set; } = string.Empty;
    public string GlobalFontSize { get; set; } = string.Empty;
    public string ThemeColors { get; set; } = string.Empty;
    public string Descriptions { get; set; } = string.Empty;
    public string AppKey { get; set; } = string.Empty;
    public string RealmKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string DbVersion { get; set; } = string.Empty;
    public string AppLogo { get; set; } = string.Empty;
    public string KeySalt { get; set; } = string.Empty;
    public string AllowCrossOrigin { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public DatabaseType DatabaseType { get; set; } = DatabaseType.PostgreSQL;
}

public class UpdateProjectApplicationDto : BaseUpdateDto
{
    public required string Name { get; set; }
    public string DefaultTheme { get; set; } = string.Empty;
    public string DefaultLanguage { get; set; } = string.Empty;
    public string GlobalFontSize { get; set; } = string.Empty;
    public string ThemeColors { get; set; } = string.Empty;
    public string Descriptions { get; set; } = string.Empty;
    public string AppKey { get; set; } = string.Empty;
    public string RealmKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string DbVersion { get; set; } = string.Empty;
    public string AppLogo { get; set; } = string.Empty;
    public string KeySalt { get; set; } = string.Empty;
    public string AllowCrossOrigin { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public DatabaseType DatabaseType { get; set; } = DatabaseType.PostgreSQL;
}

public class ProjectApplicationDto : BaseDto
{
    public required string Name { get; set; }
    public string DefaultTheme { get; set; } = string.Empty;
    public string DefaultLanguage { get; set; } = string.Empty;
    public string GlobalFontSize { get; set; } = string.Empty;
    public string ThemeColors { get; set; } = string.Empty;
    public string Descriptions { get; set; } = string.Empty;
    public string AppKey { get; set; } = string.Empty;
    public string RealmKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string DbVersion { get; set; } = string.Empty;
    public string AppLogo { get; set; } = string.Empty;
    public string KeySalt { get; set; } = string.Empty;
    public string AllowCrossOrigin { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public DatabaseType DatabaseType { get; set; } = DatabaseType.PostgreSQL;
}

public class ProjectApplicationListDto : BaseListDto
{
    public required string Name { get; set; }
    public string Descriptions { get; set; } = string.Empty;
    public string AppKey { get; set; } = string.Empty;
    public string DbVersion { get; set; } = string.Empty;
}
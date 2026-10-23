 
using NpgsqlTypes;

namespace SorteFederation.Models;

public partial class ProjectApplication : BaseEntity
{ 
    public required string Name { get; set; }
     
    public string DefaultTheme { get; set; }
     
    public string DefaultLanguage { get; set; }
     
    public string GlobalFontSize { get; set; }
     
    public string ThemeColors { get; set; }

    public string Descriptions { get; set; }
     
    public string AppKey { get; set; }

    public string RealmKey { get; set; }

    public string SecretKey { get; set; }
     
    public string DbVersion { get; set; }
     
    public string AppLogo { get; set; }

    public string KeySalt { get; set; }

    public string AllowCrossOrigin { get; set; }

    public string ConnectionString { get; set; } = string.Empty;

    public DatabaseType DatabaseType { get; set; } = DatabaseType.PostgreSQL;

    public NpgsqlTsVector? SearchVector { get; set; } = null;

    public virtual ICollection<ApplicationMenu> ApplicationMenus { get; set; }

    public virtual ICollection<ApplicationClaim> ApplicationClaims { get; set; }

    public virtual ICollection<ApplicationModuleIssue> ApplicationModuleIssues { get; set; }

    public virtual ICollection<ApplicationModule> ApplicationModules { get; set; }

    public virtual ICollection<ApplicationDoc> ApplicationDocs { get; set; }

    public virtual ICollection<CompanyApplication> CompanyApplications { get; set; }

    public virtual ICollection<ApplicationIntegratorKey> ApplicationIntegratorKeys { get; set; } = new HashSet<ApplicationIntegratorKey>();
    
}

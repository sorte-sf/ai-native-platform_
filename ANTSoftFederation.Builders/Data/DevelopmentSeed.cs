
namespace SorteFederation.Builders.Data;

public static class DevelopmentSeed
{
    public static readonly Guid TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static async Task SeedAsync(BuilderDbContext db, CancellationToken token = default)
    {
        if (await db.Applications.AnyAsync(x => x.TenantId == TenantId && x.Key == "sample-crm", token)) return;

        var application = new BuilderApplication
        {
            TenantId = TenantId,
            Key = "sample-crm",
            Name = "Sample CRM",
            Description = "Development-only sample application for Builders API testing."
        };
        var contact = new BuilderObject
        {
            TenantId = TenantId,
            ApplicationId = application.Id,
            Key = "contact",
            Name = "Contact"
        };
        foreach (var field in new[]
        {
            new BuilderField { TenantId = TenantId, ObjectId = contact.Id, Key = "firstName", Label = "First name", Type = BuilderFieldType.Text, Required = true, Searchable = true, SortOrder = 1 },
            new BuilderField { TenantId = TenantId, ObjectId = contact.Id, Key = "lastName", Label = "Last name", Type = BuilderFieldType.Text, Required = true, Searchable = true, SortOrder = 2 },
            new BuilderField { TenantId = TenantId, ObjectId = contact.Id, Key = "email", Label = "Email", Type = BuilderFieldType.Email, Required = true, Searchable = true, SortOrder = 3 },
            new BuilderField { TenantId = TenantId, ObjectId = contact.Id, Key = "status", Label = "Status", Type = BuilderFieldType.Choice, ConfigurationJson = "{\"options\":[\"lead\",\"customer\"]}", Searchable = true, SortOrder = 4 }
        }) contact.Fields.Add(field);
        contact.Forms.Add(new BuilderForm
        {
            TenantId = TenantId,
            ObjectId = contact.Id,
            Key = "default",
            Name = "Contact form",
            LayoutJson = "{\"columns\":2,\"fields\":[\"firstName\",\"lastName\",\"email\",\"status\"]}"
        });
        application.Objects.Add(contact);
        var workflow = new BuilderWorkflow
        {
            TenantId = TenantId,
            ApplicationId = application.Id,
            Key = "contact-review",
            Name = "Contact review",
            DefinitionJson = "{\"tasks\":[\"Review contact\",\"Approve contact\"]}"
        };
        var report = new BuilderReport
        {
            TenantId = TenantId,
            ApplicationId = application.Id,
            ObjectId = contact.Id,
            Key = "all-contacts",
            Name = "All contacts",
            DefinitionJson = "{\"columns\":[\"firstName\",\"lastName\",\"email\",\"status\"]}"
        };
        var dashboard = new BuilderDashboard
        {
            TenantId = TenantId,
            ApplicationId = application.Id,
            Key = "crm-overview",
            Name = "CRM overview"
        };
        dashboard.Widgets.Add(new BuilderDashboardWidget
        {
            TenantId = TenantId,
            DashboardId = dashboard.Id,
            Key = "contact-count",
            Type = "record-count",
            ConfigurationJson = $"{{\"objectId\":\"{contact.Id}\"}}",
            SortOrder = 1
        });
        var rule = new BuilderRule
        {
            TenantId = TenantId,
            ApplicationId = application.Id,
            Key = "lead-requires-email",
            Name = "Lead requires email",
            DefinitionJson = "{\"all\":[{\"field\":\"status\",\"operator\":\"eq\",\"value\":\"lead\"},{\"field\":\"email\",\"operator\":\"isNotNull\"}],\"actions\":[\"contact.email.validated\"]}"
        };

        db.Applications.Add(application);
        db.Workflows.Add(workflow);
        db.Reports.Add(report);
        db.Dashboards.Add(dashboard);
        db.Rules.Add(rule);
        await db.SaveChangesAsync(token);
    }

}

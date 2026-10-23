
namespace SorteFederation.Builders.Data;

/// <summary>Creates and maintains the single built-in CRM starter in the Builder catalogue.</summary>
public static class BuiltInCrmTemplate
{
    private const string Key = "crm-starter";
    private const string Description = "A ready-to-customize CRM with a sales dashboard, accounts, contacts, leads, opportunities, and activities.";

    private static readonly JsonSerializerOptions ManifestOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task SeedAsync(BuilderDbContext db, CancellationToken token = default)
    {
        var manifest = CreateManifest();
        var manifestJson = JsonSerializer.Serialize(manifest, ManifestOptions);
        _ = TemplateManifestService.Parse(manifestJson);

        const string themeJson = """
            {"primaryColor":"#2447a8","backgroundColor":"#f4f6fb","surfaceColor":"#ffffff","textColor":"#14213d","fontFamily":"Inter, Arial, sans-serif","fontSize":14,"fontScale":1,"shell":{"brand":"Sorte CRM","profileName":"User","profileRole":"Sales team","language":"EN","showSearch":true,"showMegaMenu":false,"showNotifications":true,"showSettings":true,"showThemeToggle":true,"themeMode":"light","sidebarCollapsed":false,"sidebarPosition":"left","sidebarWidth":252,"headerPosition":"top","headerHeight":62,"sidebarBackground":"#0d1738","sidebarTextColor":"#f4f6ff","headerBackground":"#ffffff","headerTextColor":"#14213d"}}
            """;
        var validatedTheme = TemplateThemeService.Validate(themeJson);

        var template = await db.Templates.IgnoreQueryFilters()
            .Include(x => x.Releases)
            .SingleOrDefaultAsync(x => x.Key == Key, token);
        var isNew = template is null;
        template ??= new BuilderTemplate
        {
            Key = Key,
            Name = "CRM Starter",
            HtmlContent = string.Empty,
            ScssContent = string.Empty
        };

        template.Name = "CRM Starter";
        template.Category = "CRM";
        template.Description = Description;
        template.PreviewImageUrl = "/browser/templates/sales-crm.svg";
        template.ManifestVersion = manifest.Version;
        template.ManifestJson = manifestJson;
        template.ThemeJson = validatedTheme;
        template.HtmlContent = DashboardHtml;
        template.ScssContent = string.Empty;
        template.BaseTemplate = "fuse-angular-22";
        template.IsBuiltIn = true;
        template.RecordState = RecordState.Active;
        template.UpdatedAt = DateTimeOffset.UtcNow;

        if (isNew) db.Templates.Add(template);

        // A built-in template is immutable once released. If an older CRM seed
        // already used this version, keep its history and publish the refreshed
        // built-in snapshot under the next unused patch version.
        var version = string.IsNullOrWhiteSpace(template.BlueprintVersion) ? "1.0.0" : template.BlueprintVersion;
        while (template.Releases.Any(release => release.ReleaseVersion == version && !Matches(release, template)))
            version = TemplateReleaseService.NextPatch(version);
        template.BlueprintVersion = version;

        var releaseSnapshot = await TemplateReleaseService.EnsureReleaseAsync(db, template, token);
        if (!template.Releases.Any(release => release.Id == releaseSnapshot.Id))
            template.Releases.Add(releaseSnapshot);

        await db.SaveChangesAsync(token);
    }

    private static bool Matches(BuilderTemplateRelease release, BuilderTemplate template) =>
        release.ManifestVersion == template.ManifestVersion && release.ManifestJson == template.ManifestJson &&
        release.ThemeJson == template.ThemeJson && release.HtmlContent == template.HtmlContent &&
        release.ScssContent == template.ScssContent && release.Description == template.Description &&
        release.PreviewImageUrl == template.PreviewImageUrl && release.BaseTemplate == template.BaseTemplate;

    private static TemplateManifest CreateManifest()
    {
        static TemplateField Field(string key, string label, BuilderFieldType type, bool required = false,
            bool searchable = false, string? configuration = null) =>
            new(key, label, type, required, searchable, configuration);
        static TemplateForm Form(string key, string name, TemplateField[] definitions, params string[] fieldKeys)
        {
            var fieldsByKey = definitions.ToDictionary(field => field.Key, StringComparer.OrdinalIgnoreCase);
            var fields = fieldKeys.Select(fieldKey =>
            {
                var field = fieldsByKey[fieldKey];
                var type = field.Type switch
                {
                    BuilderFieldType.LongText => "textarea",
                    BuilderFieldType.Integer or BuilderFieldType.Decimal => "input",
                    BuilderFieldType.Boolean => "checkbox",
                    BuilderFieldType.Date or BuilderFieldType.DateTime => "datepicker",
                    BuilderFieldType.SelectionBox or BuilderFieldType.Choice or BuilderFieldType.RadioButton or BuilderFieldType.Lookup => "select",
                    BuilderFieldType.MultiChoice => "multi-checkbox",
                    _ => "input"
                };
                var configuration = field.ConfigurationJson is null
                    ? new Dictionary<string, JsonElement>()
                    : JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(field.ConfigurationJson) ?? [];
                var properties = new Dictionary<string, object?>();
                if (field.Type == BuilderFieldType.Email) properties["type"] = "email";
                if (field.Type is BuilderFieldType.Integer or BuilderFieldType.Decimal) properties["type"] = "number";
                return new Dictionary<string, object?>
                {
                    ["key"] = field.Key,
                    ["label"] = field.Label,
                    ["type"] = type,
                    ["required"] = field.Required,
                    ["options"] = configuration.TryGetValue("options", out var options) ? options : null,
                    ["props"] = properties
                };
            }).ToList();
            fields.Add(new Dictionary<string, object?>
            {
                ["key"] = "submit", ["label"] = "Save", ["type"] = "submit",
                ["props"] = new Dictionary<string, object?> { ["color"] = "primary" }
            });
            return new(key, name, JsonSerializer.Serialize(new { fields }));
        }

        static TemplateObject Object(string key, string name, TemplateField[] fields, string formKey, string formName, params string[] formFields) =>
            new(key, name, fields, [Form(formKey, formName, fields, formFields)]);
        const string accountLookup = "{\"lookup\":{\"targetObjectKey\":\"accounts\",\"displayField\":\"name\"}}";

        var objects = new TemplateObject[]
        {
            Object("accounts", "Accounts", [
                Field("name", "Account name", BuilderFieldType.Text, true, true),
                Field("industry", "Industry", BuilderFieldType.SelectionBox, searchable: true,
                    configuration: "{\"source\":\"static\",\"options\":[\"Technology\",\"Healthcare\",\"Finance\",\"Manufacturing\",\"Retail\",\"Other\"]}"),
                Field("website", "Website", BuilderFieldType.Url),
                Field("phone", "Phone", BuilderFieldType.Phone),
                Field("annual-revenue", "Annual revenue", BuilderFieldType.Decimal),
                Field("status", "Status", BuilderFieldType.SelectionBox, searchable: true,
                    configuration: "{\"source\":\"static\",\"options\":[\"Prospect\",\"Customer\",\"Partner\",\"Inactive\"]}")
            ], "account-details", "Account details", "name", "industry", "website", "phone", "annual-revenue", "status"),
            Object("contacts", "Contacts", [
                Field("first-name", "First name", BuilderFieldType.Text, true, true),
                Field("last-name", "Last name", BuilderFieldType.Text, true, true),
                Field("email", "Email", BuilderFieldType.Email, searchable: true),
                Field("phone", "Phone", BuilderFieldType.Phone),
                Field("job-title", "Job title", BuilderFieldType.Text),
                Field("account", "Account", BuilderFieldType.Lookup, configuration: accountLookup)
            ], "contact-details", "Contact details", "first-name", "last-name", "email", "phone", "job-title", "account"),
            Object("leads", "Leads", [
                Field("first-name", "First name", BuilderFieldType.Text, true, true),
                Field("last-name", "Last name", BuilderFieldType.Text, true, true),
                Field("company", "Company", BuilderFieldType.Text, true, true),
                Field("email", "Email", BuilderFieldType.Email, searchable: true),
                Field("phone", "Phone", BuilderFieldType.Phone),
                Field("source", "Source", BuilderFieldType.SelectionBox,
                    configuration: "{\"source\":\"static\",\"options\":[\"Website\",\"Referral\",\"Campaign\",\"Event\",\"Other\"]}"),
                Field("status", "Status", BuilderFieldType.SelectionBox, searchable: true,
                    configuration: "{\"source\":\"static\",\"options\":[\"New\",\"Working\",\"Qualified\",\"Converted\",\"Unqualified\"]}")
            ], "lead-details", "Lead details", "first-name", "last-name", "company", "email", "phone", "source", "status"),
            Object("opportunities", "Opportunities", [
                Field("name", "Opportunity", BuilderFieldType.Text, true, true),
                Field("account", "Account", BuilderFieldType.Lookup, configuration: accountLookup),
                Field("stage", "Stage", BuilderFieldType.SelectionBox, true, searchable: true,
                    configuration: "{\"source\":\"static\",\"options\":[\"Prospecting\",\"Qualified\",\"Proposal\",\"Negotiation\",\"Won\",\"Lost\"]}"),
                Field("amount", "Amount", BuilderFieldType.Decimal, true),
                Field("close-date", "Close date", BuilderFieldType.Date),
                Field("probability", "Probability", BuilderFieldType.Integer),
                Field("owner", "Owner", BuilderFieldType.User, searchable: true)
            ], "opportunity-details", "Opportunity details", "name", "account", "stage", "amount", "close-date", "probability", "owner"),
            Object("activities", "Activities", [
                Field("subject", "Subject", BuilderFieldType.Text, true, true),
                Field("type", "Activity type", BuilderFieldType.SelectionBox, searchable: true,
                    configuration: "{\"source\":\"static\",\"options\":[\"Call\",\"Meeting\",\"Email\",\"Task\"]}"),
                Field("due-date", "Due date", BuilderFieldType.Date),
                Field("status", "Status", BuilderFieldType.SelectionBox, searchable: true,
                    configuration: "{\"source\":\"static\",\"options\":[\"Open\",\"In progress\",\"Completed\",\"Cancelled\"]}"),
                Field("contact", "Contact", BuilderFieldType.Lookup,
                    configuration: "{\"lookup\":{\"targetObjectKey\":\"contacts\",\"displayField\":\"email\"}}"),
                Field("notes", "Notes", BuilderFieldType.LongText)
            ], "activity-details", "Activity details", "subject", "type", "due-date", "status", "contact", "notes")
        };

        var screens = new TemplateScreen[]
        {
            new("dashboard", "Sales dashboard", "opportunities", string.Empty,
                Icon: "['fas','chart-line']", DefinitionJson: DashboardDefinition),
            new("accounts", "Accounts", "accounts", "<h1>Accounts</h1><p>Manage organizations, key details, and account health.</p>", "account-details", "['fas','building']"),
            new("contacts", "Contacts", "contacts", "<h1>Contacts</h1><p>Keep decision-makers and customer relationships in one place.</p>", "contact-details", "['fas','address-book']"),
            new("leads", "Leads", "leads", "<h1>Leads</h1><p>Capture new prospects and track them through qualification.</p>", "lead-details", "['fas','user-plus']"),
            new("opportunities", "Opportunities", "opportunities", "<h1>Opportunities</h1><p>Track pipeline value, deal stage, owners, and close dates.</p>", "opportunity-details", "['fas','handshake']"),
            new("activities", "Activities", "activities", "<h1>Activities</h1><p>Plan and follow up on calls, meetings, and customer tasks.</p>", "activity-details", "['fas','calendar-check']")
        };

        var navigation = new TemplateNavigationItem[]
        {
            new("dashboard", "Dashboard", "['fas','chart-line']", "dashboard"),
            new("customers", "Customers", "['fas','users']", null, [
                new("accounts", "Accounts", "['fas','building']", "accounts"),
                new("contacts", "Contacts", "['fas','address-book']", "contacts")
            ]),
            new("sales", "Sales", "['fas','bullseye']", null, [
                new("leads", "Leads", "['fas','user-plus']", "leads"),
                new("opportunities", "Opportunities", "['fas','handshake']", "opportunities")
            ]),
            new("activities", "Activities", "['fas','calendar-check']", "activities")
        };

        return new TemplateManifest(1, objects, screens, Navigation: navigation);
    }

    private const string DashboardHtml = """
        <section aria-label="CRM sales dashboard">
          <p>SALES WORKSPACE</p>
          <h1>Good morning, {{application.name}}</h1>
          <p>Your customer relationships and sales pipeline at a glance.</p>
          <p>Counts, charts, and the sales pipeline below reflect records entered in this application.</p>
          <div class="crm-dashboard-grid">
            <article><h2>Opportunity pipeline</h2><p>See opportunities grouped by their current stage.</p><p>Use the Opportunities page to manage deal value, stage, owner, and expected close date.</p></article>
            <article><h2>Customer activity</h2><p>Review calls, meetings, and follow-ups from the Activities page.</p><p>Customer records: {{records.count}}</p></article>
          </div>
          <p>Start by adding your accounts and contacts, then connect leads to opportunities.</p>
        </section>
        """;

    private const string DashboardDefinition = """
        {
          "schemaVersion": 1,
          "children": [
            {
              "id": "crm-dashboard",
              "type": "container",
              "children": [
                { "id": "dashboard-title", "type": "heading", "text": "Sales overview" },
                { "id": "dashboard-welcome", "type": "text", "text": "Your customer relationships and sales pipeline at a glance." },
                {
                  "id": "dashboard-metrics",
                  "type": "container",
                  "responsiveColumns": { "desktop": [8, 8, 8], "tablet": [12, 12], "mobile": [24] },
                  "children": [
                    { "id": "metric-opportunities", "type": "kpi", "text": "Opportunity records", "objectKey": "opportunities" },
                    { "id": "metric-leads", "type": "kpi", "text": "Lead records", "objectKey": "leads" },
                    { "id": "metric-accounts", "type": "kpi", "text": "Customer accounts", "objectKey": "accounts" }
                  ]
                },
                {
                  "id": "dashboard-charts",
                  "type": "container",
                  "responsiveColumns": { "desktop": [8, 8, 8], "tablet": [12, 12], "mobile": [24] },
                  "children": [
                    {
                      "id": "opportunity-stage-chart",
                      "type": "chart",
                      "text": "Opportunity value by stage",
                      "objectKey": "opportunities",
                      "columns": ["stage"],
                      "valueField": "amount",
                      "chartKind": "bar"
                    },
                    {
                      "id": "lead-source-chart",
                      "type": "chart",
                      "text": "Leads by source",
                      "objectKey": "leads",
                      "columns": ["source"],
                      "chartKind": "doughnut"
                    },
                    {
                      "id": "account-industry-chart",
                      "type": "chart",
                      "text": "Accounts by industry",
                      "objectKey": "accounts",
                      "columns": ["industry"],
                      "chartKind": "pie"
                    }
                  ]
                },
                { "id": "pipeline-heading", "type": "heading", "text": "Opportunity pipeline" },
                {
                  "id": "opportunity-board",
                  "type": "kanban",
                  "text": "Sales pipeline",
                  "objectKey": "opportunities",
                  "columns": ["stage"],
                  "titleField": "name"
                },
                {
                  "id": "recent-opportunities",
                  "type": "table",
                  "text": "Recent opportunities",
                  "objectKey": "opportunities",
                  "columns": ["name", "account", "stage", "amount", "close-date", "owner"]
                }
              ]
            }
          ]
        }
        """;
}

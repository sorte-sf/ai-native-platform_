namespace SorteFederation.Builders.Data;

/// <summary>Seeds the built-in purchasing starter and immutable releases in the Builder catalogue.</summary>
public static class BuiltInPurchaseTemplate
{
    private const string Key = "purchase-starter";
    private const string Description = "A responsive purchasing workspace for requests, approvals, suppliers, purchase orders, and receiving.";
    private static readonly JsonSerializerOptions ManifestOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task SeedAsync(BuilderDbContext db, CancellationToken token = default)
    {
        var manifestJson = JsonSerializer.Serialize(CreateManifest(), ManifestOptions);
        var manifest = TemplateManifestService.Parse(manifestJson);
        const string themeJson = """
            {"primaryColor":"#2447a8","backgroundColor":"#f4f6fb","surfaceColor":"#ffffff","textColor":"#14213d","fontFamily":"Inter, Arial, sans-serif","fontSize":14,"fontScale":1,"shell":{"brand":"Sorte Purchasing","profileName":"User","profileRole":"Purchasing team","language":"EN","showSearch":true,"showMegaMenu":false,"showNotifications":true,"showSettings":true,"showThemeToggle":true,"themeMode":"light","sidebarCollapsed":false,"sidebarPosition":"left","sidebarWidth":252,"headerPosition":"top","headerHeight":62,"sidebarBackground":"#0d1738","sidebarTextColor":"#f4f6ff","headerBackground":"#ffffff","headerTextColor":"#14213d"}}
            """;
        var validatedTheme = TemplateThemeService.Validate(themeJson);
        var template = await db.Templates.IgnoreQueryFilters().Include(x => x.Releases)
            .SingleOrDefaultAsync(x => x.Key == Key, token);
        var isNew = template is null;
        template ??= new BuilderTemplate { Key = Key, Name = "Purchase Starter", HtmlContent = "", ScssContent = "" };
        template.Name = "Purchase Starter";
        template.Category = "Purchasing";
        template.Description = Description;
        template.PreviewImageUrl = "/browser/templates/purchase-management.svg";
        template.ManifestVersion = manifest.Version;
        template.ManifestJson = manifestJson;
        template.ThemeJson = validatedTheme;
        template.HtmlContent = DashboardHtml;
        template.ScssContent = "";
        template.BaseTemplate = "fuse-angular-22";
        template.IsBuiltIn = true;
        template.RecordState = RecordState.Active;
        template.UpdatedAt = DateTimeOffset.UtcNow;
        if (isNew) db.Templates.Add(template);

        // Preserve immutable releases; when the seed definition changes, advance the patch version.
        var version = string.IsNullOrWhiteSpace(template.BlueprintVersion) ? "1.0.0" : template.BlueprintVersion;
        while (template.Releases.Any(release => release.ReleaseVersion == version && !Matches(release, template)))
            version = TemplateReleaseService.NextPatch(version);
        template.BlueprintVersion = version;
        var release = await TemplateReleaseService.EnsureReleaseAsync(db, template, token);
        if (!template.Releases.Any(item => item.Id == release.Id)) template.Releases.Add(release);
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
            bool searchable = false, string? configuration = null) => new(key, label, type, required, searchable, configuration);
        static string Options(params string[] values) => JsonSerializer.Serialize(new { source = "static", options = values });
        static string AdminOptions(params string[] values) => JsonSerializer.Serialize(new { source = "static", options = values,
            permissions = new { writeRoles = new[] { "SystemAdmin", "TenantAdmin" } } });
        const string adminOnly = "{\"permissions\":{\"writeRoles\":[\"SystemAdmin\",\"TenantAdmin\"]}}";
        const string approvalLimitConfiguration = "{\"min\":0,\"permissions\":{\"writeRoles\":[\"SystemAdmin\",\"TenantAdmin\"]}}";
        static TemplateObject Object(string key, string name, TemplateField[] fields)
        {
            var formFields = fields.Select(field => new Dictionary<string, object?>
            {
                ["key"] = field.Key,
                ["label"] = field.Label,
                ["type"] = field.Type switch
                {
                    BuilderFieldType.LongText => "textarea",
                    BuilderFieldType.Integer or BuilderFieldType.Decimal => "input",
                    BuilderFieldType.Boolean or BuilderFieldType.Checkbox => "checkbox",
                    BuilderFieldType.Date or BuilderFieldType.DateTime => "datepicker",
                    BuilderFieldType.SelectionBox or BuilderFieldType.Choice or BuilderFieldType.RadioButton or BuilderFieldType.Lookup => "select",
                    BuilderFieldType.MultiChoice => "multi-checkbox",
                    _ => "input"
                },
                ["required"] = field.Required,
                ["options"] = ReadOptions(field.ConfigurationJson),
                ["props"] = field.Type switch
                {
                    BuilderFieldType.Email => new Dictionary<string, object?> { ["type"] = "email" },
                    BuilderFieldType.Decimal or BuilderFieldType.Integer => new Dictionary<string, object?> { ["type"] = "number" },
                    _ => new Dictionary<string, object?>()
                }
            }).ToList();
            formFields.Add(new Dictionary<string, object?> { ["key"] = "submit", ["label"] = "Save", ["type"] = "submit" });
            var formJson = JsonSerializer.Serialize(new { fields = formFields });
            return new TemplateObject(key, name, fields, [new TemplateForm($"{key}-details", $"{name} details", formJson)]);
        }
        static JsonElement? ReadOptions(string? json)
        {
            if (json is null) return null;
            using var document = JsonDocument.Parse(json);
            return document.RootElement.TryGetProperty("options", out var options) ? options.Clone() : null;
        }
        static TemplateScreen Screen(string key, string title, string objectKey, string formKey, string icon,
            string? definition = null) => new(key, title, objectKey,
                $"<h1>{title}</h1><p>Review and manage {title.ToLowerInvariant()} for your purchasing workspace.</p>", formKey, icon, definition);

        const string supplierLookup = "{\"lookup\":{\"targetObjectKey\":\"suppliers\",\"displayField\":\"name\"}}";
        const string employeeLookup = "{\"lookup\":{\"targetObjectKey\":\"employees\",\"displayField\":\"personnel-code\"}}";
        const string requestLookup = "{\"lookup\":{\"targetObjectKey\":\"purchase-requests\",\"displayField\":\"request-number\"}}";
        const string orderLookup = "{\"lookup\":{\"targetObjectKey\":\"purchase-orders\",\"displayField\":\"order-number\"}}";
        const string requestLineLookup = "{\"lookup\":{\"targetObjectKey\":\"request-lines\",\"displayField\":\"description\"}}";
        const string orderLineLookup = "{\"lookup\":{\"targetObjectKey\":\"purchase-order-lines\",\"displayField\":\"description\"}}";
        const string receiptLookup = "{\"lookup\":{\"targetObjectKey\":\"goods-receipts\",\"displayField\":\"receipt-number\"}}";

        var objects = new TemplateObject[]
        {
            Object("suppliers", "Suppliers", [
                Field("name", "Supplier name", BuilderFieldType.Text, true, true),
                Field("supplier-code", "Supplier code", BuilderFieldType.Text, searchable: true),
                Field("tax-number", "Tax number", BuilderFieldType.Text),
                Field("email", "Email", BuilderFieldType.Email, searchable: true),
                Field("phone", "Phone", BuilderFieldType.Phone),
                Field("payment-terms", "Payment terms", BuilderFieldType.Text),
                Field("status", "Status", BuilderFieldType.SelectionBox, configuration: Options("Active", "On hold", "Inactive"))
            ]),
            Object("purchase-requests", "Purchase requests", [
                Field("request-number", "Request number", BuilderFieldType.Text, true, true),
                Field("title", "Request title", BuilderFieldType.Text, true, true),
                Field("request-type", "Request type", BuilderFieldType.SelectionBox, true, configuration: Options("Goods", "Service", "Capital expenditure", "Direct purchase")),
                Field("requester", "Requester", BuilderFieldType.User, required: true),
                Field("requester-employee", "Requesting employee", BuilderFieldType.Lookup, required: true, configuration: employeeLookup),
                Field("org-unit", "Department", BuilderFieldType.Lookup, searchable: true,
                    configuration: "{\"lookup\":{\"targetObjectKey\":\"departments\",\"displayField\":\"name\"}}"),
                Field("cost-center", "Cost center", BuilderFieldType.Text, searchable: true),
                Field("status", "Status", BuilderFieldType.SelectionBox, true, searchable: true, configuration: Options("Draft", "Submitted", "In approval", "Approved", "Rejected", "Withdrawn", "Cancelled")),
                Field("total-estimate", "Estimated total", BuilderFieldType.Decimal, true),
                Field("needed-by", "Needed by", BuilderFieldType.Date),
                Field("justification", "Business justification", BuilderFieldType.LongText, true),
                Field("risk-flags", "Risk flags", BuilderFieldType.MultiChoice, configuration: Options("Urgent", "Single supplier", "Off contract", "Capital expenditure")),
                Field("suggested-supplier", "Suggested supplier", BuilderFieldType.Lookup, configuration: supplierLookup)
            ]),
            Object("request-lines", "Request lines", [
                Field("purchase-request", "Purchase request", BuilderFieldType.Lookup, true, configuration: requestLookup),
                Field("line-number", "Line number", BuilderFieldType.Integer, true),
                Field("product", "Product", BuilderFieldType.Lookup, configuration: "{\"lookup\":{\"targetObjectKey\":\"products\",\"displayField\":\"name\"}}"),
                Field("product-vendor", "Selected product vendor", BuilderFieldType.Lookup, configuration: "{\"lookup\":{\"targetObjectKey\":\"product-vendors\",\"displayField\":\"vendor-product-code\"}}"),
                Field("description", "Item or service", BuilderFieldType.Text, true, true),
                Field("category", "Category", BuilderFieldType.SelectionBox, searchable: true, configuration: Options("Technology", "Facilities", "Professional services", "Travel", "Office supplies", "Other")),
                Field("quantity", "Quantity", BuilderFieldType.Decimal, true),
                Field("unit", "Unit", BuilderFieldType.SelectionBox, configuration: Options("Each", "Hour", "Day", "Month", "Lot")),
                Field("estimated-unit-price", "Estimated unit price", BuilderFieldType.Decimal, true),
                Field("line-total", "Estimated line total", BuilderFieldType.Decimal,
                    configuration: "{\"formula\":{\"left\":\"quantity\",\"right\":\"estimated-unit-price\",\"operator\":\"*\"}}"),
                Field("suggested-supplier", "Suggested supplier", BuilderFieldType.Lookup, configuration: supplierLookup),
                Field("status", "Line status", BuilderFieldType.SelectionBox, configuration: Options("Open", "In sourcing", "Ordered", "Cancelled"))
            ]),
            Object("approval-tasks", "Approval tasks", [
                Field("task-title", "Task title", BuilderFieldType.Text, true, true),
                Field("purchase-request", "Purchase request", BuilderFieldType.Lookup, true, configuration: requestLookup),
                Field("stage", "Approval stage", BuilderFieldType.SelectionBox, true, configuration: Options("Request review", "Supplier selection", "Order release")),
                Field("step-order", "Step order", BuilderFieldType.Integer, true),
                Field("approver", "Approver", BuilderFieldType.User),
                Field("authority-level", "Authority level", BuilderFieldType.SelectionBox, configuration: Options("L1", "L2", "L3", "L4", "L5")),
                Field("status", "Task status", BuilderFieldType.SelectionBox, true, searchable: true, configuration: Options("Pending", "Approved", "Rejected", "Sent back", "Skipped")),
                Field("due-at", "Due date", BuilderFieldType.DateTime),
                Field("decision", "Decision", BuilderFieldType.SelectionBox, configuration: Options("Approve", "Reject", "Send back")),
                Field("comments", "Comments", BuilderFieldType.LongText)
            ]),
            Object("purchase-orders", "Purchase orders", [
                Field("order-number", "Order number", BuilderFieldType.Text, true, true),
                Field("supplier", "Supplier", BuilderFieldType.Lookup, true, configuration: supplierLookup),
                Field("purchase-request", "Source request", BuilderFieldType.Lookup, configuration: requestLookup),
                Field("status", "Order status", BuilderFieldType.SelectionBox, true, searchable: true, configuration: Options("Draft", "Approved", "Sent", "Confirmed", "Partially received", "Closed", "Cancelled")),
                Field("total-amount", "Order total", BuilderFieldType.Decimal, true),
                Field("payment-terms", "Payment terms", BuilderFieldType.Text),
                Field("delivery-terms", "Delivery terms", BuilderFieldType.Text),
                Field("erp-document-reference", "ERP document reference", BuilderFieldType.Text)
            ]),
            Object("purchase-order-lines", "Purchase order lines", [
                Field("purchase-order", "Purchase order", BuilderFieldType.Lookup, true, configuration: orderLookup),
                Field("request-line", "Request line", BuilderFieldType.Lookup, configuration: requestLineLookup),
                Field("description", "Item or service", BuilderFieldType.Text, true, true),
                Field("quantity", "Quantity", BuilderFieldType.Decimal, true),
                Field("unit", "Unit", BuilderFieldType.Text),
                Field("unit-price", "Unit price", BuilderFieldType.Decimal, true),
                Field("line-total", "Line total", BuilderFieldType.Decimal,
                    configuration: "{\"formula\":{\"left\":\"quantity\",\"right\":\"unit-price\",\"operator\":\"*\"}}"),
                Field("quantity-received", "Quantity received", BuilderFieldType.Decimal)
            ]),
            Object("goods-receipts", "Goods receipts", [
                Field("receipt-number", "Receipt number", BuilderFieldType.Text, true, true),
                Field("purchase-order", "Purchase order", BuilderFieldType.Lookup, true, configuration: orderLookup),
                Field("delivery-note-number", "Delivery note number", BuilderFieldType.Text),
                Field("received-at", "Received at", BuilderFieldType.DateTime),
                Field("received-by", "Received by", BuilderFieldType.User),
                Field("status", "Receipt status", BuilderFieldType.SelectionBox, true, configuration: Options("Draft", "Posted", "Cancelled")),
                Field("erp-document-reference", "ERP document reference", BuilderFieldType.Text)
            ]),
            Object("goods-receipt-lines", "Goods receipt lines", [
                Field("goods-receipt", "Goods receipt", BuilderFieldType.Lookup, true, configuration: receiptLookup),
                Field("purchase-order-line", "Purchase order line", BuilderFieldType.Lookup, configuration: orderLineLookup),
                Field("description", "Item or service", BuilderFieldType.Text, true, true),
                Field("quantity-received", "Quantity received", BuilderFieldType.Decimal, true),
                Field("quantity-accepted", "Quantity accepted", BuilderFieldType.Decimal),
                Field("quantity-rejected", "Quantity rejected", BuilderFieldType.Decimal),
                Field("difference-type", "Difference type", BuilderFieldType.SelectionBox, configuration: Options("None", "Short", "Excess", "Off order", "Damaged")),
                Field("difference-note", "Difference note", BuilderFieldType.LongText)
            ]),
            Object("quote-requests", "Quote requests", [
                Field("quote-request-number", "Quote request number", BuilderFieldType.Text, true, true),
                Field("purchase-request", "Purchase request", BuilderFieldType.Lookup, true, configuration: requestLookup),
                Field("status", "Status", BuilderFieldType.SelectionBox, true, searchable: true, configuration: Options("Draft", "Inviting", "Collecting quotes", "Evaluation", "Awarded", "Cancelled")),
                Field("response-deadline", "Response deadline", BuilderFieldType.DateTime),
                Field("notes", "Notes", BuilderFieldType.LongText)
            ]),
            Object("supplier-quotes", "Supplier quotes", [
                Field("quote-request", "Quote request", BuilderFieldType.Lookup, true, configuration: "{\"lookup\":{\"targetObjectKey\":\"quote-requests\",\"displayField\":\"quote-request-number\"}}"),
                Field("supplier", "Supplier", BuilderFieldType.Lookup, true, configuration: supplierLookup),
                Field("version", "Version", BuilderFieldType.Integer, true),
                Field("valid-until", "Valid until", BuilderFieldType.Date),
                Field("total-amount", "Total amount", BuilderFieldType.Decimal, true),
                Field("payment-terms", "Payment terms", BuilderFieldType.Text),
                Field("delivery-terms", "Delivery terms", BuilderFieldType.Text),
                Field("status", "Quote status", BuilderFieldType.SelectionBox, true, searchable: true, configuration: Options("Submitted", "Correction requested", "Superseded"))
            ]),
            Object("supplier-quote-lines", "Supplier quote lines", [
                Field("supplier-quote", "Supplier quote", BuilderFieldType.Lookup, true, configuration: "{\"lookup\":{\"targetObjectKey\":\"supplier-quotes\",\"displayField\":\"total-amount\"}}"),
                Field("request-line", "Requested item", BuilderFieldType.Lookup, true, configuration: requestLineLookup),
                Field("unit-price", "Unit price", BuilderFieldType.Decimal, true),
                Field("quantity", "Quantity", BuilderFieldType.Decimal, true),
                Field("lead-time-days", "Lead time (days)", BuilderFieldType.Integer),
                Field("notes", "Notes", BuilderFieldType.LongText)
            ]),
            Object("supplier-selections", "Supplier selections", [
                Field("quote-request", "Quote request", BuilderFieldType.Lookup, true, configuration: "{\"lookup\":{\"targetObjectKey\":\"quote-requests\",\"displayField\":\"quote-request-number\"}}"),
                Field("supplier-quote", "Selected supplier quote", BuilderFieldType.Lookup, configuration: "{\"lookup\":{\"targetObjectKey\":\"supplier-quotes\",\"displayField\":\"total-amount\"}}"),
                Field("selected-by", "Selected by", BuilderFieldType.User),
                Field("status", "Selection status", BuilderFieldType.SelectionBox, true, configuration: Options("In review", "Selected", "Cancelled")),
                Field("rationale", "Selection rationale", BuilderFieldType.LongText, true),
                Field("single-supplier", "Single supplier", BuilderFieldType.Boolean),
                Field("off-contract", "Off contract", BuilderFieldType.Boolean),
                Field("quote-variance-flag", "Quote variance flagged", BuilderFieldType.Boolean),
                Field("selected-at", "Selected at", BuilderFieldType.DateTime)
            ]),
            Object("invoices", "Supplier invoices", [
                Field("invoice-number", "Invoice number", BuilderFieldType.Text, true, true),
                Field("supplier", "Supplier", BuilderFieldType.Lookup, true, configuration: supplierLookup),
                Field("purchase-order", "Purchase order", BuilderFieldType.Lookup, configuration: orderLookup),
                Field("invoice-date", "Invoice date", BuilderFieldType.Date, true),
                Field("due-date", "Due date", BuilderFieldType.Date),
                Field("net-amount", "Net amount", BuilderFieldType.Decimal),
                Field("tax-amount", "Tax amount", BuilderFieldType.Decimal),
                Field("gross-amount", "Gross amount", BuilderFieldType.Decimal, true),
                Field("status", "Invoice status", BuilderFieldType.SelectionBox, true, searchable: true, configuration: Options("Received", "Matching", "Matched", "Disputed", "Approved for posting", "Posted", "Rejected"))
            ]),
            Object("invoice-match-results", "Invoice match results", [
                Field("invoice", "Invoice", BuilderFieldType.Lookup, true, configuration: "{\"lookup\":{\"targetObjectKey\":\"invoices\",\"displayField\":\"invoice-number\"}}"),
                Field("result", "Match result", BuilderFieldType.SelectionBox, true, configuration: Options("Full", "Within tolerance", "Mismatch")),
                Field("price-variance-percent", "Price variance (%)", BuilderFieldType.Decimal),
                Field("quantity-variance-percent", "Quantity variance (%)", BuilderFieldType.Decimal),
                Field("matched-at", "Matched at", BuilderFieldType.DateTime)
            ]),
            Object("departments", "Departments", [
                Field("name", "Department name", BuilderFieldType.Text, true, true),
                Field("department-code", "Department code", BuilderFieldType.Text, true, true),
                Field("department-head", "Department head", BuilderFieldType.Lookup, configuration: "{\"lookup\":{\"targetObjectKey\":\"employees\",\"displayField\":\"personnel-code\"}}"),
                Field("active", "Active", BuilderFieldType.Boolean, true)
            ]),
            Object("employees", "Employees", [
                Field("name", "First name", BuilderFieldType.Text, true, true),
                Field("surname", "Surname", BuilderFieldType.Text, true, true),
                Field("personnel-code", "Personnel code", BuilderFieldType.Text, true, true),
                Field("department", "Department", BuilderFieldType.Lookup, true, configuration: "{\"lookup\":{\"targetObjectKey\":\"departments\",\"displayField\":\"name\"}}"),
                Field("job-title", "Job title", BuilderFieldType.Text, true, true),
                Field("manager", "Reports to", BuilderFieldType.Lookup, configuration: "{\"lookup\":{\"targetObjectKey\":\"employees\",\"displayField\":\"personnel-code\"}}"),
                Field("work-email", "Work email", BuilderFieldType.Email, searchable: true),
                Field("portal-user-id", "Portal user ID", BuilderFieldType.Text, configuration: adminOnly),
                Field("approval-limit", "Approval limit (local currency)", BuilderFieldType.Decimal, true, configuration: approvalLimitConfiguration),
                Field("active", "Active", BuilderFieldType.Boolean)
            ]),
            Object("employee-absences", "Employee absences", [
                Field("employee", "Employee", BuilderFieldType.Lookup, true, configuration: "{\"lookup\":{\"targetObjectKey\":\"employees\",\"displayField\":\"personnel-code\"},\"permissions\":{\"writeRoles\":[\"SystemAdmin\",\"TenantAdmin\"]}}"),
                Field("absence-type", "Absence type", BuilderFieldType.SelectionBox, true, configuration: AdminOptions("Vacation", "Sick leave", "Other")),
                Field("start-date", "Start date", BuilderFieldType.Date, true, configuration: adminOnly),
                Field("end-date", "End date", BuilderFieldType.Date, true, configuration: adminOnly),
                Field("status", "Status", BuilderFieldType.SelectionBox, true, configuration: AdminOptions("Scheduled", "Active", "Cancelled")),
                Field("notes", "Notes", BuilderFieldType.LongText, configuration: adminOnly)
            ]),
            Object("approval-delegations", "Temporary approval assignments", [
                Field("absent-employee", "Absent employee", BuilderFieldType.Lookup, true, configuration: "{\"lookup\":{\"targetObjectKey\":\"employees\",\"displayField\":\"personnel-code\"},\"permissions\":{\"writeRoles\":[\"SystemAdmin\",\"TenantAdmin\"]}}"),
                Field("substitute-employee", "Substitute employee", BuilderFieldType.Lookup, true, configuration: "{\"lookup\":{\"targetObjectKey\":\"employees\",\"displayField\":\"personnel-code\"},\"permissions\":{\"writeRoles\":[\"SystemAdmin\",\"TenantAdmin\"]}}"),
                Field("start-date", "Coverage starts", BuilderFieldType.Date, true, configuration: adminOnly),
                Field("end-date", "Coverage ends", BuilderFieldType.Date, true, configuration: adminOnly),
                Field("status", "Status", BuilderFieldType.SelectionBox, true, configuration: AdminOptions("Active", "Cancelled")),
                Field("notes", "Notes", BuilderFieldType.LongText, configuration: adminOnly)
            ]),
            Object("products", "Products", [
                Field("sku", "Product code", BuilderFieldType.Text, true, true),
                Field("name", "Product name", BuilderFieldType.Text, true, true),
                Field("description", "Description", BuilderFieldType.LongText),
                Field("category", "Category", BuilderFieldType.Text, searchable: true),
                Field("unit", "Unit", BuilderFieldType.Text),
                Field("active", "Active", BuilderFieldType.Boolean)
            ]),
            Object("product-vendors", "Product vendors", [
                Field("product", "Product", BuilderFieldType.Lookup, true, configuration: "{\"lookup\":{\"targetObjectKey\":\"products\",\"displayField\":\"name\"}}"),
                Field("vendor", "Vendor", BuilderFieldType.Lookup, true, configuration: supplierLookup),
                Field("vendor-product-code", "Vendor product code", BuilderFieldType.Text, searchable: true),
                Field("unit-price", "Unit price (local currency)", BuilderFieldType.Decimal, true),
                Field("price-valid-from", "Price valid from", BuilderFieldType.Date),
                Field("price-valid-until", "Price valid until", BuilderFieldType.Date),
                Field("preferred", "Preferred vendor", BuilderFieldType.Boolean),
                Field("lead-time-days", "Lead time (days)", BuilderFieldType.Integer),
                Field("active", "Active", BuilderFieldType.Boolean)
            ])
        };

        var dashboard = new TemplateScreen("dashboard", "Procurement overview", "purchase-requests", "", "purchase-requests-details",
            "['fas','chart-line']", DashboardDefinition);
        var screens = new TemplateScreen[]
        {
            dashboard,
            Screen("requests", "Purchase requests", "purchase-requests", "purchase-requests-details", "['fas','file-circle-plus']"),
            Screen("approvals", "Approval inbox", "approval-tasks", "approval-tasks-details", "['fas','check-double']"),
            Screen("suppliers", "Suppliers", "suppliers", "suppliers-details", "['fas','truck-field']"),
            Screen("orders", "Purchase orders", "purchase-orders", "purchase-orders-details", "['fas','file-invoice']"),
            Screen("receiving", "Receiving", "goods-receipts", "goods-receipts-details", "['fas','boxes-stacked']"),
            Screen("sourcing", "Sourcing", "quote-requests", "quote-requests-details", "['fas','file-signature']"),
            Screen("supplier-quotes", "Supplier quotes", "supplier-quotes", "supplier-quotes-details", "['fas','file-contract']"),
            Screen("supplier-selections", "Supplier selections", "supplier-selections", "supplier-selections-details", "['fas','award']"),
            Screen("invoices", "Invoices", "invoices", "invoices-details", "['fas','file-invoice-dollar']"),
            Screen("request-lines", "Requested products", "request-lines", "request-lines-details", "['fas','cart-plus']"),
            Screen("departments", "Departments", "departments", "departments-details", "['fas','sitemap']"),
            Screen("employees", "Employees", "employees", "employees-details", "['fas','users']"),
            Screen("employee-absences", "Employee absences", "employee-absences", "employee-absences-details", "['fas','calendar-minus']"),
            Screen("approval-delegations", "Approval coverage", "approval-delegations", "approval-delegations-details", "['fas','user-clock']"),
            Screen("products", "Products", "products", "products-details", "['fas','box']"),
            Screen("product-vendors", "Product vendors", "product-vendors", "product-vendors-details", "['fas','truck']")
        };
        var navigation = new TemplateNavigationItem[]
        {
            new("dashboard", "Dashboard", "['fas','chart-line']", "dashboard"),
            new("purchasing", "Purchasing", "['fas','cart-shopping']", null, [
                new("requests", "Purchase requests", "['fas','file-circle-plus']", "requests"),
                new("request-lines", "Requested products", "['fas','cart-plus']", "request-lines"),
                new("approvals", "Approval inbox", "['fas','check-double']", "approvals")
            ]),
            new("supply-chain", "Supply chain", "['fas','truck-fast']", null, [
                new("suppliers", "Suppliers", "['fas','truck-field']", "suppliers"),
                new("orders", "Purchase orders", "['fas','file-invoice']", "orders"),
                new("receiving", "Receiving", "['fas','boxes-stacked']", "receiving")
            ]),
            new("catalog", "Product catalog", "['fas','box']", null, [
                new("products", "Products", "['fas','box']", "products"),
                new("product-vendors", "Product vendors", "['fas','truck']", "product-vendors")
            ]),
            new("sourcing-menu", "Sourcing", "['fas','file-signature']", null, [
                new("sourcing", "Quote requests", "['fas','file-signature']", "sourcing"),
                new("supplier-quotes", "Supplier quotes", "['fas','file-contract']", "supplier-quotes"),
                new("supplier-selections", "Supplier selections", "['fas','award']", "supplier-selections")
            ]),
            new("organization", "Organization", "['fas','sitemap']", null, [
                new("departments", "Departments", "['fas','sitemap']", "departments"),
                new("employees", "Employees", "['fas','users']", "employees"),
                new("employee-absences", "Employee absences", "['fas','calendar-minus']", "employee-absences"),
                new("approval-delegations", "Approval coverage", "['fas','user-clock']", "approval-delegations")
            ]),
            new("finance", "Finance", "['fas','file-invoice-dollar']", null, [
                new("invoices", "Invoices", "['fas','file-invoice-dollar']", "invoices")
            ])
        };
        var relationships = new TemplateRelationship[]
        {
            new("supplier-orders", "Supplier purchase orders", "suppliers", "purchase-orders", "one-to-many"),
            new("request-lines", "Request lines", "purchase-requests", "request-lines", "one-to-many"),
            new("request-orders", "Orders from request", "purchase-requests", "purchase-orders", "one-to-many"),
            new("order-lines", "Purchase order lines", "purchase-orders", "purchase-order-lines", "one-to-many"),
            new("order-receipts", "Goods receipts", "purchase-orders", "goods-receipts", "one-to-many"),
            new("receipt-lines", "Goods receipt lines", "goods-receipts", "goods-receipt-lines", "one-to-many"),
            new("request-quote-requests", "Sourcing events", "purchase-requests", "quote-requests", "one-to-many"),
            new("sourcing-quotes", "Supplier responses", "quote-requests", "supplier-quotes", "one-to-many"),
            new("supplier-quotes", "Supplier quotes", "suppliers", "supplier-quotes", "one-to-many"),
            new("quote-lines", "Quote lines", "supplier-quotes", "supplier-quote-lines", "one-to-many"),
            new("quote-selection", "Awarded selection", "quote-requests", "supplier-selections", "one-to-one"),
            new("invoice-order", "Invoice purchase order", "purchase-orders", "invoices", "one-to-many"),
            new("invoice-match", "Invoice match result", "invoices", "invoice-match-results", "one-to-one"),
            new("employees-departments", "Employees by department", "departments", "employees", "one-to-many"),
            new("department-head", "Department head", "employees", "departments", "many-to-one"),
            new("employee-manager", "Employee reporting line", "employees", "employees", "many-to-one"),
            new("employee-absence", "Employee absences", "employees", "employee-absences", "one-to-many"),
            new("absence-delegation", "Temporary approval coverage", "employees", "approval-delegations", "one-to-many"),
            new("product-vendor", "Vendors for products", "products", "product-vendors", "one-to-many"),
            new("line-product", "Product on request line", "products", "request-lines", "one-to-many"),
            new("line-product-vendor", "Vendor selection for request line", "product-vendors", "request-lines", "one-to-many")
        };
        var workflows = new TemplateWorkflow[]
        {
            new("purchase-request-approval", "Purchase request approval", """
                {
                  "purchaseApproval": {
                    "objectKey": "purchase-requests",
                    "statusField": "status",
                    "requesterField": "requester",
                    "routing": {
                      "employeeObjectKey": "employees",
                      "departmentObjectKey": "departments",
                      "absenceObjectKey": "employee-absences",
                      "delegationObjectKey": "approval-delegations",
                      "requesterEmployeeField": "requester-employee",
                      "managerField": "manager",
                      "approvalLimitField": "approval-limit",
                      "portalUserIdField": "portal-user-id",
                      "departmentField": "department",
                      "departmentHeadField": "department-head"
                    },
                    "submittedStatus": "Draft",
                    "pendingStatus": "In approval",
                    "approvedStatus": "Approved",
                    "rejectedStatus": "Rejected"
                  },
                  "tasks": [
                    { "name": "Purchase request review", "assignedTo": "TenantAdmin" }
                  ]
                }
                """)
        };
        return new TemplateManifest(1, objects, screens, Workflows: workflows, Navigation: navigation, Relationships: relationships);
    }

    private const string DashboardHtml = """
        <section aria-label="Purchasing dashboard">
          <p>PROCUREMENT WORKSPACE</p>
          <h1>Procurement overview</h1>
          <p>Follow purchase demand, approvals, supplier commitments, and receiving progress.</p>
          <p>Dashboard counts and charts use records entered in this application. Create a purchase request to get started.</p>
        </section>
        """;

    private const string DashboardDefinition = """
        {
          "schemaVersion": 1,
          "children": [
            {
              "id": "purchasing-dashboard",
              "type": "container",
              "children": [
                { "id": "procurement-title", "type": "heading", "text": "Procurement overview" },
                { "id": "procurement-intro", "type": "text", "text": "Track purchasing demand, approvals, supplier commitments, and delivery progress." },
                {
                  "id": "procurement-kpis",
                  "type": "container",
                  "responsiveColumns": { "desktop": [8, 8, 8], "tablet": [12, 12], "mobile": [24] },
                  "children": [
                    { "id": "kpi-requests", "type": "kpi", "text": "Purchase requests", "objectKey": "purchase-requests" },
                    { "id": "kpi-suppliers", "type": "kpi", "text": "Supplier records", "objectKey": "suppliers" },
                    { "id": "kpi-orders", "type": "kpi", "text": "Purchase orders", "objectKey": "purchase-orders" }
                  ]
                },
                {
                  "id": "procurement-charts",
                  "type": "container",
                  "responsiveColumns": { "desktop": [8, 8, 8], "tablet": [12, 12], "mobile": [24] },
                  "children": [
                    { "id": "request-status-chart", "type": "chart", "text": "Requests by status", "objectKey": "purchase-requests", "columns": ["status"], "chartKind": "doughnut" },
                    { "id": "request-type-chart", "type": "chart", "text": "Requests by type", "objectKey": "purchase-requests", "columns": ["request-type"], "chartKind": "bar" },
                    { "id": "order-status-chart", "type": "chart", "text": "Orders by status", "objectKey": "purchase-orders", "columns": ["status"], "chartKind": "bar" }
                  ]
                },
                { "id": "approval-heading", "type": "heading", "text": "Approval work" },
                { "id": "approval-board", "type": "kanban", "text": "Approval task pipeline", "objectKey": "approval-tasks", "columns": ["status"], "titleField": "task-title" },
                { "id": "recent-requests", "type": "table", "text": "Purchase request register", "objectKey": "purchase-requests", "columns": ["request-number", "title", "request-type", "requester", "status", "total-estimate", "needed-by"] }
              ]
            }
          ]
        }
        """;
}

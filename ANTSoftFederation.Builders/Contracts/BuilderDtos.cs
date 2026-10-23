using SorteFederation.Builders.Data.Enums;

namespace SorteFederation.Builders.Contracts;

public sealed record CreateApplicationRequest(
    Guid TenantId,
    string Key,
    string Name,
    string? Description,
    BuilderApplicationType ApplicationType,
    Guid? TemplateId = null,
    string? Icon = null,
    CreateInitialScreenRequest? InitialScreen = null,
    string? ThemeJson = null,
    CreateInitialFormRequest? InitialForm = null,
    IReadOnlyList<CreateInitialModuleRequest>? Modules = null,
    string VersionNo = "0.0.1",
    string PublishedVersion = "0.0.1",
    string[]? Slugs = null
    );

public sealed record CreateInitialModuleRequest(string Key, string Name, string ScreenKey, string? Icon = null, string? Description = null, bool Visible = true);
public sealed record CreateApplicationModuleRequest(Guid TenantId, Guid ApplicationId, string Key, string Name, string? Description = null);
public sealed record UpdateApplicationModuleRequest(Guid TenantId, string Key, string Name, string? Description = null);
public sealed record BindApplicationModuleRequest(Guid TenantId, Guid ScreenId, Guid ObjectId, Guid FormId);
public sealed record CreateInitialScreenRequest(string Key, string Title, string HtmlContent);
public sealed record CreateInitialFormRequest(string ObjectKey, string ObjectName, string FormKey, string FormName, string? LayoutJson, IReadOnlyList<SaveDesignerFieldRequest> Fields);
public sealed record CreateApplicationMenuItemRequest(Guid TenantId, Guid ApplicationId, Guid? ParentId, string Key, string Label, string? Route, string? Icon, int SortOrder = 0, bool Visible = true, string? PermissionKey = null, Guid? ModuleId = null);
public sealed record UpdateApplicationMenuItemRequest(Guid TenantId, Guid? ParentId, string Key, string Label, string? Route, string? Icon, int SortOrder = 0, bool Visible = true, string? PermissionKey = null, Guid? ModuleId = null);
public sealed record CreateTemplateRequest(string Name, string? Description, string HtmlContent, string ScssContent, string? ManifestJson = null, string? ThemeJson = null);
public sealed record UpdateTemplateRequest(string Name, string? Description, string HtmlContent, string ScssContent, string? ManifestJson = null, string? ThemeJson = null);
public sealed record CreateObjectRequest(Guid TenantId, Guid ApplicationId, string Key, string Name);
public sealed record CreateFieldRequest(Guid TenantId, Guid ObjectId, string Key, string Label, BuilderFieldType Type, int SortOrder, bool Required, bool Searchable, bool ReadOnly, string? ConfigurationJson);
public sealed record CreateFormRequest(Guid TenantId, Guid ObjectId, string Key, string Name, string? LayoutJson);
public sealed record CreateRecordRequest(Guid TenantId, Guid ObjectId, string DataJson);
public sealed record UpdateRecordRequest(Guid TenantId, string DataJson, int Version);
public sealed record CreateRelationshipRequest(Guid TenantId, Guid SourceObjectId, Guid TargetObjectId, string Key, string Name, string Cardinality);
public sealed record CreatePermissionRequest(Guid TenantId, Guid ApplicationId, string Key, string Name, string ResourceType, string Action);
public sealed record GrantPermissionRequest(Guid TenantId, Guid PermissionId, string SubjectType, string SubjectId);
public sealed record CreateWorkflowRequest(Guid TenantId, Guid ApplicationId, string Key, string Name, string DefinitionJson);
public sealed record UpdateWorkflowRequest(Guid TenantId, string Name, string DefinitionJson);
public sealed record CreateConnectorRequest(Guid TenantId, Guid ApplicationId, string Key, string Name, string Type, string ConfigurationJson);
public sealed record PublishApplicationRequest(Guid TenantId, Guid ApplicationId);
public sealed record CreateRecordRelationshipRequest(Guid TenantId, Guid RelationshipId, Guid SourceRecordId, Guid TargetRecordId);
public sealed record StartWorkflowRequest(Guid TenantId, Guid WorkflowId, Guid RecordId);
public sealed record SubmitPurchaseApprovalRequest(Guid TenantId, Guid WorkflowId, Guid RecordId);
public sealed record PurchaseApprovalDecisionRequest(Guid TenantId, string Decision, string? Comment = null);
public sealed record CompleteWorkflowTaskRequest(Guid TenantId, Guid TaskId);
public sealed record AssignWorkflowTaskRequest(Guid TenantId, string AssignedTo);
public sealed record ExecuteConnectorRequest(Guid TenantId, Guid ConnectorId, string? PayloadJson);
public sealed record CreateWorkflowTransitionRequest(Guid TenantId, Guid WorkflowId, string FromState, string ToState, string Action, string? ConditionJson);
public sealed record TransitionWorkflowRequest(Guid TenantId, Guid InstanceId, string Action);
public sealed record CreateReportRequest(Guid TenantId, Guid ApplicationId, Guid ObjectId, string Key, string Name, string DefinitionJson);
public sealed record CreateDashboardRequest(Guid TenantId, Guid ApplicationId, string Key, string Name, string LayoutJson);
public sealed record CreateDashboardWidgetRequest(Guid TenantId, Guid DashboardId, string Key, string Type, string ConfigurationJson, int SortOrder);
public sealed record CreateCredentialRequest(Guid TenantId, Guid ConnectorId, string SecretReference, string Scheme);
public sealed record CreateAuditEventRequest(Guid TenantId, string EventType, string ResourceType, Guid? ResourceId, string? DataJson);
public sealed record ApproveApplicationVersionRequest(Guid TenantId, Guid VersionId, bool Approve, string? Reason);
public sealed record ValidateFormRequest(Guid TenantId, Guid ObjectId, string DataJson, bool ForWrite = true);
public sealed record FormValidationResponse(bool Valid, IReadOnlyList<string> Errors);
public sealed record RuntimeFormFieldResponse(string Key, string Label, BuilderFieldType Type, int SortOrder, bool Required, bool ReadOnly, string? ConfigurationJson);
public sealed record RuntimeFormDefinitionResponse(Guid FormId, Guid ObjectId, string Key, string Name, string? LayoutJson, IReadOnlyList<RuntimeFormFieldResponse> Fields);
public sealed record DesignerApplicationResponse(Guid Id, Guid TenantId, string Key, string Name, string Status, IReadOnlyList<DesignerObjectResponse> Objects,
    Guid? TemplateId = null, string? TemplateHtml = null, string? TemplateScss = null, IReadOnlyList<DesignerModuleResponse>? Modules = null, string? CreatedByUserId = null);
public sealed record DesignerModuleResponse(Guid Id, string Key, string Name, Guid? ScreenId, Guid? ObjectId, Guid? FormId, DesignerObjectResponse? Design);
public sealed record DesignerObjectResponse(Guid Id, string Key, string Name, string Status, Guid? FormId, string? FormKey, string? FormName, string? LayoutJson, IReadOnlyList<DesignerFieldResponse> Fields, IReadOnlyList<DesignerFormResponse>? Forms = null);
public sealed record DesignerFormResponse(Guid Id, string Key, string Name, string? LayoutJson);
public sealed record DesignerFieldResponse(Guid Id, string Key, string Label, BuilderFieldType Type, int SortOrder, bool Required, bool Searchable, bool ReadOnly, string? ConfigurationJson);
public sealed record SaveDesignerFieldRequest(Guid? Id, string Key, string Label, BuilderFieldType Type, int SortOrder, bool Required, bool Searchable, bool ReadOnly, string? ConfigurationJson);
public sealed record SaveDesignerDraftRequest(Guid TenantId, Guid ApplicationId, Guid? ObjectId, string ObjectKey, string ObjectName, Guid? FormId, string FormKey, string FormName, string? LayoutJson, IReadOnlyList<SaveDesignerFieldRequest> Fields, Guid? ModuleId = null, Guid? PageId = null, string? PageNodeId = null);
public sealed record PublishPlatformEventRequest(Guid TenantId, string Type, string PayloadJson);
public sealed record CreateFeatureFlagRequest(Guid TenantId, Guid ApplicationId, string Key, bool Enabled, string? RulesJson);
public sealed record CreateScheduledJobRequest(Guid TenantId, Guid ApplicationId, string Key, string Schedule, string ActionType, string ActionJson, DateTimeOffset? NextRunAt, string TimeZoneId = "Europe/Istanbul", string ScheduleType = "recurring", DateTime? LocalRunAt = null);
public sealed record DynamicFilter(string Field, string Operator, string? Value);
public sealed record DynamicQueryRequest(Guid TenantId, Guid ObjectId, IReadOnlyList<DynamicFilter>? Filters, string? SortField, bool Descending = false, int Page = 1, int PageSize = 50);
public sealed record DynamicQueryResponse(IReadOnlyList<BuilderRecord> Items, int Total, int Page, int PageSize);
public sealed record CreateKnowledgeDocumentRequest(Guid TenantId, string Key, string Title, string Content, string? Source, string? MetadataJson, Guid? ApplicationId = null);
public sealed record KnowledgeSearchRequest(Guid TenantId, string Query, int Limit = 10);
public sealed record VectorKnowledgeSearchRequest(Guid TenantId, float[] Embedding, int Limit = 10);
public sealed record SolutionDocumentSearchRequest(Guid TenantId, string Query, int Limit = 5);
public sealed record SolutionDocumentSearchResult(Guid DocumentId, string Title, string? Source, int ChunkOrdinal, string Content, double Score);
public sealed record AiPromptRequest(Guid TenantId, string Prompt, string? KnowledgeQuery = null);
public sealed record EvaluateRulesRequest(Guid TenantId, string RulesJson, string DataJson);
public sealed record EvaluateRulesResponse(bool Matched, IReadOnlyList<string> Errors, IReadOnlyList<string>? Actions = null);
public sealed record CreateRuleRequest(Guid TenantId, Guid ApplicationId, string Key, string Name, string DefinitionJson, bool Enabled = true);
public sealed record UpdateRuleRequest(Guid TenantId, string Key, string Name, string DefinitionJson, bool Enabled = true, int ExpectedVersion = 0);
public sealed record ExecuteRuleRequest(Guid TenantId, Guid RuleId, string DataJson);
public sealed record CreateNotificationRequest(Guid TenantId, string RecipientId, string Title, string Message, string? Type = null, string? ActionUrl = null);

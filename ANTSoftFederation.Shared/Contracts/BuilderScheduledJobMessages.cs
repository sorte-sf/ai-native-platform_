namespace SorteFederation.Shared.Contracts;

public sealed record BuilderScheduledJobMessage(
    Guid JobId,
    Guid TenantId,
    Guid ApplicationId,
    string Key,
    string Schedule,
    string ActionType,
    string ActionJson,
    bool Enabled);

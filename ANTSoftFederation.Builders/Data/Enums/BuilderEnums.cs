namespace SorteFederation.Builders.Data.Enums;

public enum DefinitionStatus { Draft = 1, Published, Archived }

public enum BuilderApplicationType { Web = 1, Mobile = 2 }

public enum BuilderFieldType
{
    Text = 1,
    LongText,
    Integer,
    Decimal,
    Boolean,
    Date,
    DateTime,
    Time,
    Email,
    Phone,
    Url,
    SelectionBox,
    Checkbox,
    RadioButton,
    Lookup,
    File,
    UploadFile
    ,Choice
    ,MultiChoice
    ,User
}

public enum GovernanceState { Draft = 1, Validated, PendingApproval, Approved, Rejected, Published, RolledBack }

public enum WorkflowInstanceStatus { Running, Completed, Cancelled, Failed }

public enum PlatformEventStatus { Pending, Queued, Processed, Failed }

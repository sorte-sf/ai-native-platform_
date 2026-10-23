namespace SorteFederation.GlobalEnums;

public enum NotificationStatus : byte
{
    Pending = 1,
    Send,
    Retry,
    Fail
}

namespace SorteFederation.Shared;

public static class RabbitMQTopics
{
    public const string CustomerCreated = "new_customer_created";
    public const string CustomerDeleted = "customer_deleted";
    public const string CustomerStateChanged = "customer_state_changed";

    public const string CompanyCreated = "new_company_created";
    public const string CompanyUpdated = "company_updated";
    public const string CompanyDeleted = "company_deleted";
    public const string CompanyStateChanged = "company_state_changed";
    public const string TenantSubscriptionChanged = "tenant_subscription_changed";

    public const string OrderCreated = "new_order_created";
    public const string OrderCancelled = "order_cancelled";
    public const string OrderStateChanged = "order_state_changed";

    public const string PaymentCreated = "new_payment_created";
    public const string PaymentReturned = "payment_returned";
    public const string PaymentStateChanged = "payment_state_changed";
    public const string PaymentCompleted = "payment_completed";

    public const string BasketCreated = "new_basket_created";
    public const string BasketExpired = "basket_expired";

    public const string ShipmentCreated = "new_shipment_created";
    public const string ShipmentCancelled = "shipment_cancelled";
    public const string ShipmentStateChanged = "shipment_state_changed";
    public const string ShipmentCompleted = "shipment_completed";

    public const string StoreCreated = "new_store_created";
    public const string StoreStateChanged = "store_state_changed";
    public const string StoreClosed = "store_closed";

    public const string InventoryCreated = "new_inventory_created";
    public const string InventoryCancelled = "inventory_cancelled";
    public const string InventoryCountStarted = "inventory_count_started";
    public const string InventoryCountCompleted = "inventory_count_completed";

    public const string NotificationCreated = "new_notification_created";
    public const string NotificationFailed = "notification_failed";
    public const string NotificationSent = "notification_sent";

    
}

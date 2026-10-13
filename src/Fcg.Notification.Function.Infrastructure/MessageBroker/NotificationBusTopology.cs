namespace Fcg.Notification.Function.Infrastructure.MessageBroker
{
    public class NotificationBusTopology
    {
        // Tópicos: mesmos nomes definidos com SetEntityName nas APIs publicadoras
        public const string UserCreatedTopic = "user-created";
        public const string UserRepublishedTopic = "user-republished";
        public const string PaymentApprovedTopic = "payment-approved";
        public const string PurchaseRejectedTopic = "purchase-rejected";
        public const string DeliveryFailedTopic = "delivery-failed";

        // Subscription da notification em cada tópico
        public const string NotificationSubscription = "notification";

        public const string ConnectionName = "ServiceBusConnection";
    }
}

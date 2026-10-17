namespace Fcg.Notification.Function.Domain.Entities
{
    public class ProcessedMessage
    {
        public string Key { get; private set; }
        public DateTime CreatedAt { get; private set; }

        protected ProcessedMessage() { }
        public ProcessedMessage(string key)
        {
            Key = key;
            CreatedAt = DateTime.UtcNow;
        }
    }
}

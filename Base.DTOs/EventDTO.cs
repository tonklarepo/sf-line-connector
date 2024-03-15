namespace Base.DTOs
{
    public class EventDTO
    {
        public string type { get; set; }
        public MessageDTO message { get; set; }
        public string webhookEventId { get; set; }
        public DeliveryContextDTO deliveryContext { get; set; }
        public long timestamp { get; set; }
        public SourceDTO source { get; set; }
        public string replyToken { get; set; }
        public string mode { get; set; }
    }
}
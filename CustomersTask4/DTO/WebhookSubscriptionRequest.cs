namespace CustomersTask4.DTO
{
    public class WebhookSubscriptionRequest
    {
        public string Url { get; set; }
        public string Event { get; set; }
        public string? Secret { get; set; }
    }
}

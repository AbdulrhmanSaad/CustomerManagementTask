namespace CustomersTask4.Domain;

public class WebhookSubscription : IMustHaveTenant
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Url { get; set; } = null!;
    public string Event { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Secret { get; set; }
    public string TenantId { get; set; } = null!;
    public DateTime? LastTriggeredAt { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
}

public class WebhookPayload<T>
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Event { get; set; }
    public T Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
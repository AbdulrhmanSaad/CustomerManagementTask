using CustomersTask4.Domain;

namespace CustomersTask4.Repository;

public interface IWebhookRepository
{
    Task<IEnumerable<WebhookSubscription>> GetAllByEventAsync(string eventType, CancellationToken cancellationToken = default);
    Task<WebhookSubscription?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task AddAsync(WebhookSubscription subscription, CancellationToken cancellationToken = default);
    Task UpdateAsync(WebhookSubscription subscription, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<WebhookSubscription>> GetAllAsync(CancellationToken cancellationToken = default);
}
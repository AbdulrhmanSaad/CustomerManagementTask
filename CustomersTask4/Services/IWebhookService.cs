using CustomersTask4.Domain;
using CustomersTask4.Messages;

namespace CustomersTask4.Services;

public interface IWebhookService
{
    Task TriggerCustomerCreatedWebhookAsync(BaseCustomerMessage customer, CancellationToken cancellationToken = default);
    Task TriggerCustomerUpdatedWebhookAsync(BaseCustomerMessage customer, CancellationToken cancellationToken = default);
    Task RegisterWebhookAsync(WebhookSubscription subscription, CancellationToken cancellationToken = default);
    Task UnregisterWebhookAsync(string subscriptionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<WebhookSubscription>> GetAllWebhooksAsync(CancellationToken cancellationToken = default);
}


using CustomersTask4.Domain;
using CustomersTask4.Messages;
using CustomersTask4.Repository;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CustomersTask4.Services;

public class WebhookService : IWebhookService
{
    private readonly ILogger<WebhookService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IWebhookRepository _repository;

    public WebhookService(
        ILogger<WebhookService> logger,
        HttpClient httpClient,
        IWebhookRepository repository)
    {
        _logger = logger;
        _httpClient = httpClient;
        _repository = repository;
    }

    public async Task TriggerCustomerCreatedWebhookAsync(BaseCustomerMessage customer, CancellationToken cancellationToken = default)
    {
        await TriggerWebhookAsync("customer.created", customer, cancellationToken);
    }

    public async Task TriggerCustomerUpdatedWebhookAsync(BaseCustomerMessage customer, CancellationToken cancellationToken = default)
    {
        await TriggerWebhookAsync("customer.updated", customer, cancellationToken);
    }

    public async Task RegisterWebhookAsync(WebhookSubscription subscription, CancellationToken cancellationToken = default)
    {
        await _repository.AddAsync(subscription, cancellationToken);
        _logger.LogInformation("Webhook registered for event: {Event} to URL: {Url}", subscription.Event, subscription.Url);
    }

    public async Task UnregisterWebhookAsync(string subscriptionId, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteAsync(subscriptionId, cancellationToken);
        _logger.LogInformation("Webhook unregistered: {SubscriptionId}", subscriptionId);
    }

    public async Task<IEnumerable<WebhookSubscription>> GetAllWebhooksAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetAllAsync(cancellationToken);
    }

    private async Task TriggerWebhookAsync(string eventType, BaseCustomerMessage customer, CancellationToken cancellationToken)
    {
        var activeSubscriptions = (await _repository.GetAllByEventAsync(eventType, cancellationToken)).ToList();

        if (!activeSubscriptions.Any())
        {
            _logger.LogDebug("No active webhooks found for event: {Event}", eventType);
            return;
        }

        var payload = new WebhookPayload<BaseCustomerMessage>
        {
            Event = eventType,
            Data = customer,
            Timestamp = DateTime.UtcNow
        };

        var tasks = activeSubscriptions.Select(sub => SendWebhookAsync(sub, payload, cancellationToken));
        await Task.WhenAll(tasks);
    }

    private async Task SendWebhookAsync(WebhookSubscription subscription, WebhookPayload<BaseCustomerMessage> payload, CancellationToken cancellationToken)
    {
        try
        {
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            if (!string.IsNullOrEmpty(subscription.Secret))
            {
                var signature = GenerateHmacSignature(json, subscription.Secret);
                content.Headers.Add("X-Webhook-Signature", signature);
            }

            var response = await _httpClient.PostAsync(subscription.Url, content, cancellationToken);

            subscription.LastTriggeredAt = DateTime.UtcNow;

            if (response.IsSuccessStatusCode)
            {
                subscription.SuccessCount++;
                await _repository.UpdateAsync(subscription, cancellationToken);

                _logger.LogInformation(
                    "Webhook sent successfully for event: {Event} to {Url} (ID: {WebhookId})",
                    payload.Event, subscription.Url, payload.Id);
            }
            else
            {
                subscription.FailureCount++;
                await _repository.UpdateAsync(subscription, cancellationToken);

                _logger.LogWarning(
                    "Webhook failed for event: {Event} to {Url}. Status: {StatusCode}",
                    payload.Event, subscription.Url, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            subscription.FailureCount++;
            await _repository.UpdateAsync(subscription, cancellationToken);

            _logger.LogError(ex, "Error sending webhook for event: {Event} to {Url}", payload.Event, subscription.Url);
        }
    }

    private static string GenerateHmacSignature(string payload, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToBase64String(hash);
    }
}
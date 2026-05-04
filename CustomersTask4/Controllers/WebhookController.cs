using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomersTask4.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class WebhookController(IWebhookService webhookService, ILogger<WebhookController> logger) : ControllerBase
{
    [HttpPost("subscribe")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WebhookSubscription>> Subscribe(
        [FromBody] WebhookSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Url) || string.IsNullOrWhiteSpace(request.Event))
            return BadRequest("URL and Event are required");

        var subscription = new WebhookSubscription
        {
            Url = request.Url,
            Event = request.Event,
            Secret = request.Secret
        };

        await webhookService.RegisterWebhookAsync(subscription, cancellationToken);
        logger.LogInformation("Webhook subscription created: {SubscriptionId}", subscription.Id);

        return Ok(subscription);
    }
    [HttpDelete("unsubscribe/{subscriptionId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unsubscribe(string subscriptionId, CancellationToken cancellationToken)
    {
        var webhooks = await webhookService.GetAllWebhooksAsync(cancellationToken);
        if (!webhooks.Any(w => w.Id == subscriptionId))
            return NotFound($"Webhook subscription {subscriptionId} not found");

        await webhookService.UnregisterWebhookAsync(subscriptionId, cancellationToken);
        logger.LogInformation("Webhook subscription deleted: {SubscriptionId}", subscriptionId);

        return NoContent();
    }

    [HttpGet("subscriptions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<WebhookSubscription>>> GetSubscriptions(CancellationToken cancellationToken)
    {
        var subscriptions = await webhookService.GetAllWebhooksAsync(cancellationToken);
        return Ok(subscriptions);
    }
}

    
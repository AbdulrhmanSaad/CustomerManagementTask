using CustomersTask4.Data;
using CustomersTask4.Domain;
using Microsoft.EntityFrameworkCore;

namespace CustomersTask4.Repository;

public class WebhookRepository : IWebhookRepository
{
    private readonly ApplicationDbContext context;
    private readonly ILogger<WebhookRepository> _logger;

    public WebhookRepository(
       ApplicationDbContext dbContext,
        ILogger<WebhookRepository> logger)
    {
        context = dbContext;
        _logger = logger;
    }

    public async Task<IEnumerable<WebhookSubscription>> GetAllByEventAsync(string eventType, CancellationToken cancellationToken = default)
    {
        try
        {
            return await context.WebhookSubscriptions
                .Where(w => (w.Event == eventType || w.Event == "*"))
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving webhooks for event: {Event}", eventType);
            return Enumerable.Empty<WebhookSubscription>();
        }
    }
    public async Task<WebhookSubscription?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await context.WebhookSubscriptions
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving webhook: {Id}", id);
            return null;
        }
    }

    public async Task<IEnumerable<WebhookSubscription>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await context.WebhookSubscriptions
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all webhooks");
            return Enumerable.Empty<WebhookSubscription>();
        }
    }

    public async Task AddAsync(WebhookSubscription subscription, CancellationToken cancellationToken = default)
    {
        try
        {
            context.WebhookSubscriptions.Add(subscription);
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Webhook subscription added: {Id}", subscription.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding webhook subscription");
            throw;
        }
    }

    public async Task UpdateAsync(WebhookSubscription subscription, CancellationToken cancellationToken = default)
    {
        try
        {
            context.WebhookSubscriptions.Update(subscription);
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Webhook subscription updated: {Id}", subscription.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating webhook subscription");
            throw;
        }
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await context.WebhookSubscriptions
                .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

            if (subscription != null)
            {
                context.WebhookSubscriptions.Remove(subscription);
                await context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Webhook subscription deleted: {Id}", id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting webhook subscription");
            throw;
        }
    }
}
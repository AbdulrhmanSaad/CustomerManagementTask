using CustomersTask4.Abstraction;
using CustomersTask4.Data;
using CustomersTask4.Domain;
using CustomersTask4.Exceptions;
using CustomersTask4.Hubs;
using CustomersTask4.Messages;
using CustomersTask4.Repository;
using CustomersTask4.Services;
using CustomersTask4.Services.Caching;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Hybrid;
using System.Text.Json;
using Wolverine;

namespace CustomersTask4.CustomerHandler.Command.DeleteCustomerCommand
{
    public class DeleteCustomerCommandHandler(
        IGenericRepository<Customer> repository,
        ApplicationDbContext db,
        ILogger<DeleteCustomerCommandHandler> logger,
        IConfiguration configuration,
        IHubContext<MessageHub> hubContext,
        IAppMeditor bus,
        ILocalizationService localization,
        HybridCache cachingService)
    {
        public async Task Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Handling DeleteCustomerCommand for Customer Id: {CustomerId}", request.Id);

            var customer = await repository.GetByIdAsync(request.Id);
            if (customer == null)
                throw new NotFoundException(localization.Localize("CustomerNotFound",request.Id));

            customer.IsDeleted=true;
            db.SaveChanges();

            await cachingService.RemoveByTagAsync("CustomerTag");

            if (!configuration["DatabaseProvidor"]!.Equals("Mongo"))
            {
                var obj = JsonSerializer.Serialize(customer);
                await hubContext.Clients.All.SendAsync("ReceiveMessage", obj, "Delete Customer", cancellationToken);
                
                await bus.PublishAsync(new CustomerDeletedMessage { Id = customer.Id });

                logger.LogInformation("CustomerDeletedMessage published for {Id}", customer.Id);
            }
        }
    }
}

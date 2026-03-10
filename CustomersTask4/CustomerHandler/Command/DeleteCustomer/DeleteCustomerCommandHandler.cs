using CustomersTask4.Abstraction;
using CustomersTask4.Domain;
using CustomersTask4.Exceptions;
using CustomersTask4.Hubs;
using CustomersTask4.Messages;
using CustomersTask4.Repository;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using Wolverine;

namespace CustomersTask4.CustomerHandler.Command.DeleteCustomerCommand
{
    public class DeleteCustomerCommandHandler(
        IGenericRepository<Customer> db,
        ILogger<DeleteCustomerCommandHandler> logger,
        IConfiguration configuration,
        IHubContext<MessageHub> hubContext,
        IAppMeditor bus)
    {
        public async Task Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Handling DeleteCustomerCommand for Customer Id: {CustomerId}", request.Id);

            var customer = await db.GetByIdAsync(request.Id);
            if (customer == null)
                throw new NotFoundException($"Customer With Id={request.Id} not found");

            await db.Delete(customer);

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

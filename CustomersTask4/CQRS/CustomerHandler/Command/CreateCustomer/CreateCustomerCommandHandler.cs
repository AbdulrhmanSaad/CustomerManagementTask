using CustomersTask4.Abstraction;
using CustomersTask4.Domain;
using CustomersTask4.Exceptions;
using CustomersTask4.Hubs;
using CustomersTask4.Messages;
using CustomersTask4.Repository;
using CustomersTask4.Users;
using MapsterMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Hybrid;
using Shared.Services;
using System.Text.Json;
using Wolverine;

namespace CustomersTask4.CQRS.CustomerHandler.Command.CreateCustomer
{
    public class CreateCustomerCommandHandler(
        IGenericRepository<Customer> db,
        ILogger<CreateCustomerCommandHandler> logger,
        IMapper mapper,
        IUserContext userContext,
        IConfiguration configuration,
        IHubContext<MessageHub> hubContext,
        IAppMeditor bus,
        ILocalizationService localization,
        HybridCache cachingService)
    {
        public async Task Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creating a new customer with Data: {Data}", request);

            bool exist =db.PhoneExistsAsync(request.Phone);
            if (exist)
                throw new NotFoundException(localization.Localize("this phone number already exists"));

            var customer = mapper.Map<Customer>(request);

            var user = userContext.GetCurrentUser();
            if (user != null)
                customer.CreatedBy = user.Name;

            await db.Add(customer);

            await cachingService.RemoveByTagAsync("CustomerTag");

            if (!configuration["DatabaseProvidor"]!.Equals("Mongo"))
            {
                var createdCustomer = mapper.Map<CustomerCreatedMessage>(customer);
                var obj = JsonSerializer.Serialize(createdCustomer);
                await hubContext.Clients.All.SendAsync("ReceiveMessage", obj, "Create Customer", cancellationToken);

                await bus.PublishAsync(createdCustomer);

                logger.LogInformation("CustomerCreatedMessage published for {Name}", customer.Name);
            }
        }
    }
}

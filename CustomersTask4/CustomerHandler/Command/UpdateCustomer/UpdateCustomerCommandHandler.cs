using CustomersTask4.Abstraction;
using CustomersTask4.Domain;
using CustomersTask4.Exceptions;
using CustomersTask4.Hubs;
using CustomersTask4.Messages;
using CustomersTask4.Repository;
using CustomersTask4.Services;
using CustomersTask4.Services.Caching;
using CustomersTask4.Users;
using MapsterMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Hybrid;
using Shared.Services;
using System.Text.Json;
using Wolverine;

namespace CustomersTask4.CustomerHandler.Command.UpdateCustomer
{
    public class UpdateCustomerCommandHandler(
        IGenericRepository<Customer> db,
        ILogger<UpdateCustomerCommandHandler> logger,
        IMapper mapper,
        IUserContext userContext,
        IConfiguration configuration,
        IHubContext<MessageHub> hubContext,
        IAppMeditor bus,
        ILocalizationService localization,
        HybridCache cachingService)
    {
        public async Task Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await db.GetByIdAsync(request.Id, c => c.Addresses);

            if (customer == null)
                throw new NotFoundException(localization.Localize($"Customer with id {request.Id} not found."));

            if (db.PhoneExistsAsync(request.Phone) && customer.Phone != request.Phone)
                throw new NotFoundException(localization.Localize("CustomerPhoneExist",request.Phone));

            mapper.Map(request, customer);

            var user = userContext.GetCurrentUser();
            if (user != null)
                customer.ChangedBy = user.Name;

            customer.ChangedAt = DateTime.UtcNow;

            await db.Update(customer);
            await cachingService.RemoveByTagAsync("CustomerTag");

            if (!configuration["DatabaseProvidor"]!.Equals("Mongo"))
            {
                var updatedCustomer = mapper.Map<CustomerUpdatedMessage>(customer);
                var obj = JsonSerializer.Serialize(updatedCustomer);
                await hubContext.Clients.All.SendAsync("ReceiveMessage", obj, "Update Customer", cancellationToken);

                await bus.PublishAsync(updatedCustomer);

                logger.LogInformation("CustomerUpdatedMessage published for {Id}", customer.Id);
            }
        }
    }
}

using CustomersTask4.Domain;
using CustomersTask4.Exceptions;
using CustomersTask4.Hubs;
using CustomersTask4.Messages;
using CustomersTask4.Repository;
using CustomersTask4.Users;
using MapsterMapper;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace CustomersTask4.CustomerHandler.Command.CreateCustomer
{
    public class CreateCustomerCommandHandler(IGenericRepository<Customer> db
        , ILogger<CreateCustomerCommandHandler> logger
        , IMapper mapper,
        IUserContext userContext,
        IConfiguration configuration,
        IHubContext<MessageHub> hubContext,
        IBus bus
        ) : IRequestHandler<CreateCustomerCommand>
    {


        public async Task Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creating a new customer with Data: {Data}", request);

            bool exist = db.PhoneExistsAsync(request.Phone);

            if (exist)
                throw new NotFoundException("this phone number already exists");

            var customer = mapper.Map<Customer>(request);

            var user = userContext.GetCurrentUser();
            if (user != null)
                customer.CreatedBy = user.Name;

            await db.Add(customer);

           

            //Publish CustomerCreatedMessage to RabbitMQ 
            if (!configuration["DatabaseProvidor"]!.Equals("Mongo"))
            {
                var CreatedCustomer = mapper.Map<CustomerCreatedMessage>(customer);
                await bus.Publish(CreatedCustomer,cancellationToken);
                var obj = JsonSerializer.Serialize(CreatedCustomer);
                await hubContext.Clients.All.SendAsync("ReceiveMessage", obj,"Create Customer", cancellationToken);

                logger.LogInformation("CustomerCreatedMessage published for {Name}", customer.Name);
            }
        }
    }
}

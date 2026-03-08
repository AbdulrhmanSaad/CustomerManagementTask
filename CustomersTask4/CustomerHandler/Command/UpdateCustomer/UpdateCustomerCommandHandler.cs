using MapsterMapper;
using Castle.Core.Resource;
using CustomersTask4.Domain;
using CustomersTask4.Exceptions;
using CustomersTask4.Repository;
using CustomersTask4.Users;
using MediatR;
using MassTransit;
using CustomersTask4.Messages;

namespace CustomersTask4.CustomerHandler.Command.UpdateCustomer
{
    public class UpdateCustomerCommandHandler(
        IGenericRepository<Customer> db,
        ILogger<UpdateCustomerCommandHandler> logger,
        IMapper mapper,
        IUserContext userContext,
        IConfiguration configuration,
        IBus bus) : IRequestHandler<UpdateCustomerCommand>
    {
        public async Task Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await db.GetByIdAsync(request.Id, c => c.Addresses);

            if (customer == null)
                throw new NotFoundException($"Customer with id {request.Id} not found.");

            if (db.PhoneExistsAsync(request.Phone) && customer.Phone != request.Phone)
                throw new NotFoundException($"Phone Number: {request.Phone} already exists.");

            mapper.Map(request, customer);

            var user = userContext.GetCurrentUser();
            if (user != null)
                customer.ChangedBy = user.Name;

            customer.ChangedAt = DateTime.UtcNow;

            await db.Update(customer);
            if (!configuration["DatabaseProvidor"]!.Equals("Mongo"))
            {
                var updatedCustomer = mapper.Map<CustomerUpdatedMessage>(customer);
                await bus.Publish(updatedCustomer,cancellationToken);

                logger.LogInformation("CustomerUpdatedMessage published for {Id}", customer.Id);

            }
        }
    }
}

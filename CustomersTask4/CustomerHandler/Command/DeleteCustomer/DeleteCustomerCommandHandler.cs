using CustomersTask4.Domain;
using CustomersTask4.Exceptions;
using CustomersTask4.Messages;
using CustomersTask4.Repository;
using MassTransit;
using MediatR;

namespace CustomersTask4.CustomerHandler.Command.DeleteCustomerCommand
{
    public class DeleteCustomerCommandHandler(IGenericRepository<Customer>db
        ,ILogger<DeleteCustomerCommandHandler>logger,
        IConfiguration configuration,
        IBus bus) : IRequestHandler<DeleteCustomerCommand>
    {
        public async Task Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Handling DeleteCustomerCommand for Customer Id: {CustomerId}", request.Id);
            var customer =await db.GetByIdAsync(request.Id);
            if(customer==null)
                throw new NotFoundException($"Customer With Id={request.Id} not found");

            await db.Delete(customer);

            if (!configuration["DatabaseProvidor"]!.Equals("Mongo"))
            {
                await bus.Publish(new CustomerDeletedMessage
                {
                    Id = customer.Id,
                }, cancellationToken);

                logger.LogInformation("CustomerDeletedMessage published for {Id}", customer.Id);
            }
        }


    }
}

using CustomersTask4.Domain;
using CustomersTask4.Exceptions;
using CustomersTask4.Repository;
using CustomersTask4.Services;
using MediatR;
using System.Security.Cryptography;

namespace CustomersTask4.CustomerHandler.Command.DeleteCustomerCommand
{
    public class DeleteCustomerCommandHandler(IGenericRepository<Customer>db
        ,ILogger<DeleteCustomerCommandHandler>logger
           ) : IRequestHandler<DeleteCustomerCommand>
    {
        public async Task Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Handling DeleteCustomerCommand for Customer Id: {CustomerId}", request.Id);
            var customer =await db.GetByIdAsync(request.Id);
            if(customer==null)
                throw new NotFoundException($"Customer With Id={request.Id} not found");

            await db.Delete(customer);


        }
    }
}

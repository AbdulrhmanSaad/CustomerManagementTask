using CustomersTask4.CustomerHandler.Query.GetAllCustomers;
using CustomersTask4.CustomerHandler.Query.GetCustomerById;
using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.Exceptions;
using CustomersTask4.Repository;
using MapsterMapper;
using Mediator;
using Microsoft.AspNetCore.Components.Forms.Mapping;

namespace CustomersTask4.CustomerHandler.Query
{
    public class GetCustomerByIdQueryHandler(IGenericRepository<Customer>db,
        ILogger<GetAllCustomerQueryHandler>logger,
        IMapper mapper) : IRequestHandler<GetCustomerByIdQuery, CustomerDto?>
    {
        public async ValueTask<CustomerDto?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation($"Getting customer by id {request.id}");
            var customer = await db.GetByIdAsync(request.id, c => c.Addresses);
            
            if(customer == null)
            {
                throw new NotFoundException($"Customer with id {request.id} not found.");
            }
            return mapper.Map<CustomerDto>(customer);
        }
    }
}

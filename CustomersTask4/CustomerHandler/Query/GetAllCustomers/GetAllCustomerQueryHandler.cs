using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.Repository;
using MapsterMapper;
using MediatR;

namespace CustomersTask4.CustomerHandler.Query.GetAllCustomers
{
    public class GetAllCustomerQueryHandler(IGenericRepository<Customer>repository,
        ILogger<GetAllCustomerQueryHandler>logger,
        IMapper mapper) : IRequestHandler<GetAllCustomerQuery, IEnumerable<CustomerDto>>
    {
        public async Task<IEnumerable<CustomerDto>> Handle(GetAllCustomerQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Get All Customer");
           var customers=mapper.Map<IEnumerable<CustomerDto>>(repository.GetAll(c=>c.Addresses));
           
            return customers;
        }
    }
}

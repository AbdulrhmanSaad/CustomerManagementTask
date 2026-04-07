using CustomersTask4.Abstraction;
using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.Repository;
using CustomersTask4.Services.Caching;
using MapsterMapper;
using Microsoft.Extensions.Caching.Hybrid;

namespace CustomersTask4.CustomerHandler.Query.GetAllCustomers
{
    public class GetAllCustomerQueryHandler(IGenericRepository<Customer>repository,
        ILogger<GetAllCustomerQueryHandler>logger,
        IMapper mapper,
        HybridCache hCach)
    {
        public async Task<IEnumerable<CustomerDto>> Handle(GetAllCustomerQuery request, CancellationToken cancellationToken)
        {

            logger.LogInformation("Get All Customer");
            var customersCached = await hCach.GetOrCreateAsync("customers", async ct =>
            {
                var customers = repository.GetAll(includes: c => c.Addresses);
                var customersmaped = mapper.Map<IEnumerable<CustomerDto>>(customers);
                return customersmaped;
            }, tags: ["CustomerTag"], cancellationToken: cancellationToken);
            logger.LogInformation($"Getting all customers from cache");

            return customersCached;
            
        }
    }
}

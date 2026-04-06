using CustomersTask4.Abstraction;
using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.Repository;
using CustomersTask4.Services.Caching;
using MapsterMapper;

namespace CustomersTask4.CustomerHandler.Query.GetAllCustomers
{
    public class GetAllCustomerQueryHandler(IGenericRepository<Customer>repository,
        ILogger<GetAllCustomerQueryHandler>logger,
        IMapper mapper,
        IRedisCachingService redis)
    {
        public async Task<IEnumerable<CustomerDto>> Handle(GetAllCustomerQuery request, CancellationToken cancellationToken)
        {

            logger.LogInformation("Get All Customer");
            var customersFromCache = redis.GetData<IEnumerable<CustomerDto>>("customers");
            if (customersFromCache is not null)
            {
                logger.LogInformation("Get All Customer From Cache");
                return customersFromCache;
            }
            var customers = repository.GetAll(includes: c => c.Addresses);
            var customersmaped=mapper.Map<IEnumerable<CustomerDto>>(customers);
            redis.SetData("customers", customersmaped);
            return customersmaped;
        }
    }
}

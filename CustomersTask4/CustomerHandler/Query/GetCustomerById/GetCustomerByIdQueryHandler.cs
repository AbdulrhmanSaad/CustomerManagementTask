using CustomersTask4.CustomerHandler.Query.GetAllCustomers;
using CustomersTask4.CustomerHandler.Query.GetCustomerById;
using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.Exceptions;
using CustomersTask4.Repository;
using CustomersTask4.Services;
using CustomersTask4.Services.Caching;
using MapsterMapper;

namespace CustomersTask4.CustomerHandler.Query
{
    public class GetCustomerByIdQueryHandler(IGenericRepository<Customer>db,
        ILogger<GetAllCustomerQueryHandler>logger,
        IMapper mapper,
        ILocalizationService localization,
        IRedisCachingService cachingService)
    {
        public async Task<CustomerDto?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation($"Getting customer by id {request.id}");
            var customerFromCache = cachingService.GetData<CustomerDto>($"customer:{request.id}");
            if (customerFromCache != null)
            {
                logger.LogInformation($"Customer with id {request.id} found in cache");
                return customerFromCache;
            }
            var customer = await db.GetByIdAsync(request.id, c => c.Addresses);

            if (customer == null)
            {
                throw new NotFoundException(localization.Localize($"Customer with id {request.id} not found."));
            }
            var customerDto = mapper.Map<CustomerDto>(customer);
            cachingService.SetData($"customer:{request.id}", customerDto);

            return customerDto;
        }
    }
}

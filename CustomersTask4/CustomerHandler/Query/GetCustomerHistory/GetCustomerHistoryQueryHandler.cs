using CustomersTask4.DTO;
using CustomersTask4.Exceptions;
using CustomersTask4.Repository;
using CustomersTask4.Services;
using CustomersTask4.Services.Caching;
using MapsterMapper;

namespace CustomersTask4.CustomerHandler.Query.GetCustomerHistory
{
    public class GetCustomerHistoryQueryHandler(ILogger<GetCustomerHistoryQueryHandler>logger,
        ICustomerHistoryRepository repository,
        IMapper mapper,
        ILocalizationService localization,
        IRedisCachingService cachingService
            ) 
    {
        public async Task<IEnumerable<CustomerHistoryResponse>> Handle(GetCustomerHistoryQuery request, CancellationToken cancellationToken)
        {
            var customrfromcache = cachingService.GetData<IEnumerable<CustomerHistoryResponse>>($"CustomerHistory_{request.CustomerId}");
            if (customrfromcache != null)
            {
                logger.LogInformation($"Customer history for customer id {request.CustomerId} retrieved from cache.");
                return customrfromcache;
            }
            var customer =await repository.GetByIdAsync(request.CustomerId);

            if (customer == null)
                throw new NotFoundException(localization.Localize($"Customer with id {request.CustomerId} not found."));

            var CustomerUpdates=await repository.GetAllCustomerHistory(request.CustomerId);


            var res =mapper.Map<IEnumerable<CustomerHistoryResponse >>(CustomerUpdates);
            cachingService.SetData($"CustomerHistory_{request.CustomerId}", res);

            return res;
        }
    }
}

using CustomersTask4.DTO;
using CustomersTask4.Exceptions;
using CustomersTask4.Repository;
using MapsterMapper;
using Microsoft.Extensions.Caching.Hybrid;
using Shared.Services;

namespace CustomersTask4.CustomerHandler.Query.GetCustomerHistory
{
    public class GetCustomerHistoryQueryHandler(ILogger<GetCustomerHistoryQueryHandler>logger,
        ICustomerHistoryRepository repository,
        IMapper mapper,
        ILocalizationService localization,
        HybridCache cachingService
            ) 
    {
        public async Task<IEnumerable<CustomerHistoryResponse>> Handle(GetCustomerHistoryQuery request, CancellationToken cancellationToken)
        {
            var caching = await cachingService.GetOrCreateAsync($"customer:{request.CustomerId} History", async ct =>
            {

                var customer = await repository.GetByIdAsync(request.CustomerId);

                if (customer == null)
                    throw new NotFoundException(localization.Localize($"Customer with id {request.CustomerId} not found."));

                var CustomerUpdates = await repository.GetAllCustomerHistory(request.CustomerId);


                var res = mapper.Map<IEnumerable<CustomerHistoryResponse>>(CustomerUpdates);
                logger.LogInformation("Getting Customer History for customer with id {CustomerId} from Database", request.CustomerId);
                return res;
            }, tags: ["CustomerTag"], cancellationToken: cancellationToken);
            logger.LogInformation("Getting Customer History for customer with id {CustomerId} from caching", request.CustomerId);
            return caching;
        }
    }
}

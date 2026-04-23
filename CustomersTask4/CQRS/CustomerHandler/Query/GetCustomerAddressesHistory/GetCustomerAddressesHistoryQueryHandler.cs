using CustomersTask4.DTO;
using CustomersTask4.Exceptions;
using CustomersTask4.Repository;
using MapsterMapper;
using Microsoft.Extensions.Caching.Hybrid;
using Shared.Services;

namespace CustomersTask4.CQRS.CustomerHandler.Query.GetCustomerAddressesHistory
{
    public class GetCustomerAddressesHistoryQueryHandler(ICustomerHistoryRepository repository,
        IMapper mapper,
        ILogger<GetCustomerAddressesHistoryQueryHandler> logger,
        ILocalizationService localization,
        HybridCache cachingService)
        
    {
        
        public async Task<IEnumerable<AddressDto>> Handle(GetCustomerAddressesHistoryQuery request, CancellationToken cancellationToken)
        {
          var cachedCustomerAddress=await cachingService.GetOrCreateAsync($"customer:{request.CustomerId} Addresses History", async ct=>
            {
                var customer =await repository.GetByIdAsync(request.CustomerId);
                if (customer == null)
                    throw new NotFoundException(localization.Localize($"Customer with id {request.CustomerId} not found."));

                var CustomerUpdates = await repository.GetAllCustomerAddressHistory(request.CustomerId);
                logger.LogInformation("Fetch customer Addresses History from Database");
                return CustomerUpdates;
            }, tags: ["CustomerTag"], cancellationToken: cancellationToken);

            logger.LogInformation("Fetch customer Addresses History from caching");
            return cachedCustomerAddress;
            
        }

   
    }
}

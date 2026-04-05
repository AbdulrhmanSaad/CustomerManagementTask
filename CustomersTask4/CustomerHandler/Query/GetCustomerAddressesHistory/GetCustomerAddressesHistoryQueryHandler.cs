using CustomersTask4.DTO;
using CustomersTask4.Exceptions;
using CustomersTask4.Repository;
using CustomersTask4.Services;
using CustomersTask4.Services.Caching;
using MapsterMapper;

namespace CustomersTask4.CustomerHandler.Query.GetCustomerAddressesHistory
{
    public class GetCustomerAddressesHistoryQueryHandler(ICustomerHistoryRepository repository,
        IMapper mapper,
        ILocalizationService localization,
        IRedisCachingService cachingService
        )
        
    {
        
        public async Task<IEnumerable<AddressDto>> Handle(GetCustomerAddressesHistoryQuery request, CancellationToken cancellationToken)
        {
            var customerAddressesHistoryFromCache = cachingService.GetData<IEnumerable<AddressDto>>($"CustomerAddressesHistory_{request.CustomerId}");
            if (customerAddressesHistoryFromCache != null)
            {
                return customerAddressesHistoryFromCache;
            }
            var customer = await repository.GetByIdAsync(request.CustomerId);
            if (customer == null)
                throw new NotFoundException(localization.Localize($"Customer with id {request.CustomerId} not found."));


            var CustomerUpdates = await repository.GetAllCustomerAddressHistory(request.CustomerId);

            cachingService.SetData($"CustomerAddressesHistory_{request.CustomerId}", CustomerUpdates);

            return CustomerUpdates;
        }

   
    }
}

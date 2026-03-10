using CustomersTask4.DTO;
using CustomersTask4.Exceptions;
using CustomersTask4.Repository;
using MapsterMapper;

namespace CustomersTask4.CustomerHandler.Query.GetCustomerAddressesHistory
{
    public class GetCustomerAddressesHistoryQueryHandler(ICustomerHistoryRepository repository, IMapper mapper)
        
    {
        
        public async Task<IEnumerable<AddressDto>> Handle(GetCustomerAddressesHistoryQuery request, CancellationToken cancellationToken)
        {

            var customer = await repository.GetByIdAsync(request.CustomerId);

            if (customer == null)
                throw new NotFoundException($"Customer with id {request.CustomerId} not found.");

            var CustomerUpdates = await repository.GetAllCustomerAddressHistory(request.CustomerId);
            return CustomerUpdates;
        }

   
    }
}

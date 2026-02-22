using AutoMapper;
using CustomersTask4.DTO;
using CustomersTask4.Repository;
using MediatR;

namespace CustomersTask4.CustomerHandler.Query.GetCustomerAddressesHistory
{
    public class GetCustomerAddressesHistoryQueryHandler:IRequestHandler<GetCustomerAddressesHistoryQuery, IEnumerable<AddressDto>>
    {
        private readonly ICustomerHistoryRepository repository;
        private readonly IMapper mapper;
        public GetCustomerAddressesHistoryQueryHandler(ICustomerHistoryRepository repository,IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }
        public async Task<IEnumerable<AddressDto>> Handle(GetCustomerAddressesHistoryQuery request, CancellationToken cancellationToken)
        {
            var CustomerUpdates = await repository.GetAllCustomerAddressHistory(request.CustomerId);
            return CustomerUpdates;
        }
    
    }
}

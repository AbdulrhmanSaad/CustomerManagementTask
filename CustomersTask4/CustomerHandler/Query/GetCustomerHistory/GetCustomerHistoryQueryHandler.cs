using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.Exceptions;
using CustomersTask4.Repository;
using CustomersTask4.Services;
using MapsterMapper;
using MediatR;

namespace CustomersTask4.CustomerHandler.Query.GetCustomerHistory
{
    public class GetCustomerHistoryQueryHandler(ILogger<GetCustomerHistoryQueryHandler>logger,
        ICustomerHistoryRepository repository,
        IMapper mapper,
        ILocalizationService localization
            ) 
    {
        public async Task<IEnumerable<CustomerHistoryResponse>> Handle(GetCustomerHistoryQuery request, CancellationToken cancellationToken)
        {

            var customer =await repository.GetByIdAsync(request.CustomerId);

            if (customer == null)
                throw new NotFoundException(localization.Localize($"Customer with id {request.CustomerId} not found."));

            var CustomerUpdates=await repository.GetAllCustomerHistory(request.CustomerId);

            var res=mapper.Map<IEnumerable<CustomerHistoryResponse >>(CustomerUpdates);
            return res;
        }
    }
}

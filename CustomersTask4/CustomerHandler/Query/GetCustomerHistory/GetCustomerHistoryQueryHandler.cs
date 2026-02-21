using AutoMapper;
using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.Repository;
using MediatR;

namespace CustomersTask4.CustomerHandler.Query.GetCustomerHistory
{
    public class GetCustomerHistoryQueryHandler(ILogger<GetCustomerHistoryQueryHandler>logger,
        ICustomerHistoryRepository repository,
        IMapper mapper) : IRequestHandler<GetCustomerHistoryQuery,IEnumerable<CustomerHistoryDto>>
    {
        public async Task<IEnumerable<CustomerHistoryDto>> Handle(GetCustomerHistoryQuery request, CancellationToken cancellationToken)
        {
           var res=await repository.GetAllCustomerHistory(request.CustomerId);
           return mapper.Map<IEnumerable<CustomerHistoryDto>>(res);
        }
    }
}

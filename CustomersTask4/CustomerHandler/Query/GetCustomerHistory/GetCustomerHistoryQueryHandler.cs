using AutoMapper;
using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.Repository;
using MediatR;

namespace CustomersTask4.CustomerHandler.Query.GetCustomerHistory
{
    public class GetCustomerHistoryQueryHandler(ILogger<GetCustomerHistoryQueryHandler>logger,
        ICustomerHistoryRepository repository,
        IMapper mapper
        ) : IRequestHandler<GetCustomerHistoryQuery,IEnumerable<CustomerHistoryResponse>>
    {
        public async Task<IEnumerable<CustomerHistoryResponse>> Handle(GetCustomerHistoryQuery request, CancellationToken cancellationToken)
        {
           var CustomerUpdates=await repository.GetAllCustomerHistory(request.CustomerId);

            var res=mapper.Map<IEnumerable<CustomerHistoryResponse >>(CustomerUpdates);



            //var address = await repository.GetAllCustomerAddressHistory(request.CustomerId);
            return res;//mapper.Map<IEnumerable<CustomerHistoryDto>>(res);
        }
    }
}

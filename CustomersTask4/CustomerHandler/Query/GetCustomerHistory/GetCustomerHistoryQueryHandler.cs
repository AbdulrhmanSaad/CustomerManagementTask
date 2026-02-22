using AutoMapper;
using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.Repository;
using MediatR;

namespace CustomersTask4.CustomerHandler.Query.GetCustomerHistory
{
    public class GetCustomerHistoryQueryHandler(ILogger<GetCustomerHistoryQueryHandler>logger,
        ICustomerHistoryRepository repository
        ) : IRequestHandler<GetCustomerHistoryQuery,IEnumerable<Customer>>
    {
        public async Task<IEnumerable<Customer>> Handle(GetCustomerHistoryQuery request, CancellationToken cancellationToken)
        {
           var res=await repository.GetAllCustomerHistory(request.CustomerId);
            //var address = await repository.GetAllCustomerAddressHistory(request.CustomerId);
            return res;//mapper.Map<IEnumerable<CustomerHistoryDto>>(res);
        }
    }
}

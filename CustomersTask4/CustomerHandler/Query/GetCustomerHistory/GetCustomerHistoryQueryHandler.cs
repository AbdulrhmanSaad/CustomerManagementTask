using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.Exceptions;
using CustomersTask4.Repository;
using MapsterMapper;
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

            var customer =await repository.GetByIdAsync(request.CustomerId);

            if (customer == null)
                throw new NotFoundException($"Customer with id {request.CustomerId} not found.");

            var CustomerUpdates=await repository.GetAllCustomerHistory(request.CustomerId);

           

            var res=mapper.Map<IEnumerable<CustomerHistoryResponse >>(CustomerUpdates);



            //var address = await repository.GetAllCustomerAddressHistory(request.CustomerId);
            return res;//mapper.Map<IEnumerable<CustomerHistoryDto>>(res);
        }
    }
}

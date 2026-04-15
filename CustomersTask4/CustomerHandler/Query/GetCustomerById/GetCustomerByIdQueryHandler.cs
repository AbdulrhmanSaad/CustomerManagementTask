using CustomersTask4.CustomerHandler.Query.GetAllCustomers;
using CustomersTask4.CustomerHandler.Query.GetCustomerById;
using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.Exceptions;
using CustomersTask4.Repository;
using MapsterMapper;
using Microsoft.Extensions.Caching.Hybrid;
using Shared.Services;

namespace CustomersTask4.CustomerHandler.Query
{
    public class GetCustomerByIdQueryHandler(IGenericRepository<Customer>db,
        ILogger<GetAllCustomerQueryHandler>logger,
        IMapper mapper,
        ILocalizationService localization,
        HybridCache cachingService)
    {
        public async Task<CustomerDto?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation($"Getting customer by id {request.id}");
            var caching=await cachingService.GetOrCreateAsync($"customer:{request.id}", async ct =>
            {
                var customer = await db.GetByIdAsync(request.id, c => c.Addresses);

                if (customer == null)
                {
                    throw new NotFoundException(localization.Localize($"Customer with id {request.id} not found."));
                }
                var customerDto = mapper.Map<CustomerDto>(customer);
                return customerDto;

            }, tags: ["GetCustomerTag"],cancellationToken: cancellationToken);
            logger.LogInformation($"Getting customer{request.id} from cache");
            return caching;
        }
    }
}

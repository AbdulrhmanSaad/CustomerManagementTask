using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.Exceptions;
using CustomersTask4.Repository;
using HotChocolate.Authorization;
using MapsterMapper;
using Shared.Services;

namespace CustomersTask4.GraphQL.Query
{
    [Authorize]
    public class CustomerManagemantQuery
    {
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IEnumerable<CustomerDto> GetCustomers(
            [Service] IGenericRepository<Customer> repo,
            [Service] IMapper mapper
            )
        {
            var customers = repo.GetAll(includes: c => c.Addresses);
            var customersmaped = mapper.Map<IEnumerable<CustomerDto>>(customers);
            return customersmaped;
        }

        [UseProjection]
        public async Task<CustomerDto> GetCustomerById(
            [Service] IGenericRepository<Customer> repo,
            string id,
            [Service] ILocalizationService localization,
            IMapper mapper)
        {            
                var customer = await repo.GetByIdAsync(id, c => c.Addresses);

                if (customer == null)
                {
                    throw new GraphQLException(localization.Localize($"Customer with id {id} not found."));
                }
                var customerDto = mapper.Map<CustomerDto>(customer);
                return customerDto;
        }
        [UseProjection]
        [UseSorting]
        [UseFiltering]
        public async Task<IEnumerable<AddressDto>> GetCustomerAddressesHistory(
            [Service] ICustomerHistoryRepository repository,
            string customerId,
            [Service] IMapper mapper,
            [Service] ILogger<CustomerManagemantQuery> logger,
            [Service] ILocalizationService localization
            )
        {
            var customer = await repository.GetByIdAsync(customerId);
            if (customer == null)
                throw new GraphQLException(localization.Localize($"Customer with id {customerId} not found."));

            var CustomerUpdates = await repository.GetAllCustomerAddressHistory(customerId);
            logger.LogInformation("Fetch customer Addresses History from Database");
            return CustomerUpdates;

        }
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public async Task<IEnumerable<CustomerHistoryResponse>> GetCustomerHistory(
            [Service] ICustomerHistoryRepository repository,
            string customerId,
            [Service] IMapper mapper,
            [Service] ILogger<CustomerManagemantQuery> logger,
            [Service] ILocalizationService localization)
        {
            var customer = await repository.GetByIdAsync(customerId);

            if (customer == null)
                throw new NotFoundException(localization.Localize($"Customer with id {customerId} not found."));

            var CustomerUpdates = await repository.GetAllCustomerHistory(customerId);


            var res = mapper.Map<IEnumerable<CustomerHistoryResponse>>(CustomerUpdates);
            logger.LogInformation("Getting Customer History for customer with id {CustomerId} from Database", customerId);
            return res;
        }
    }
}

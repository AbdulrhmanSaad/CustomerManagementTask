using CustomersTask4.Data;
using CustomersTask4.DTO;
using CustomersTask4.Exceptions;
using CustomersTask4.Repository;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Shared.Services;

namespace CustomersTask4.OData.CustomerHandlers.GetCustomerHistoryById
{
    public class GetCustomerHistoryByIdODataQueryHandler(
        ILogger<GetCustomerHistoryByIdODataQueryHandler> logger,
        IDbContextFactory<ApplicationDbContext> _dbFactory,
        ILocalizationService localization,
        IMapper mapper)
    {
        public async Task<IQueryable<CustomerHistoryResponse>> Handle(GetCustomerHistoryByIdODataQuery request, CancellationToken cancellationToken)
        {
            var db=_dbFactory.CreateDbContext();
            var customer = await db.Customers.FindAsync(request.CustomerId);

            if (customer == null)
                throw new NotFoundException(localization.Localize("CustomerNotFound",request.CustomerId));

            var CustomerUpdates = db.Customers
                .Where(u => u.Id == request.CustomerId)
                .ProjectToType<CustomerHistoryResponse>();

            logger.LogInformation("Getting Customer History for customer with id {CustomerId} from Database", request.CustomerId);
            return CustomerUpdates;
        }
    }
}

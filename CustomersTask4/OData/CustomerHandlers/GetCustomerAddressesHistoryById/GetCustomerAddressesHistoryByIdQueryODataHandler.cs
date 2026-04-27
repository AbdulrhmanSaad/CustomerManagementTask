using CustomersTask4.Data;
using CustomersTask4.DTO;
using CustomersTask4.Exceptions;
using Microsoft.EntityFrameworkCore;
using Shared.Services;

namespace CustomersTask4.OData.CustomerHandlers.GetCustomerAddressesHistoryById
{
    public class GetCustomerAddressesHistoryByIdQueryODataHandler(IDbContextFactory<ApplicationDbContext> _dbFactory,
        ILogger<GetCustomerAddressesHistoryByIdQueryODataHandler> logger,
        ILocalizationService localization
       )
    {
        public IQueryable<AddressDto> Handle(GetCustomerAddressesHistoryByIdODataQuery request)
        {
            logger.LogInformation("Fetch customer Addresses History (OData)");

            var db = _dbFactory.CreateDbContext();

            var res=db.Customers.Where(c => c.Id.Equals(request.CustomerId))
                .AsNoTracking()
                .FirstOrDefault()
                ??throw new NotFoundException(localization.Localize("CustomerNotFound", request.CustomerId));

            return
                db.Addresses.TemporalAll()
                .Where(c => c.CustomerId.Equals(request.CustomerId))
                .Select(e => new AddressDto { AddressType = e.AddressType.ToString(), AddressName = e.AddressName.ToString() })
                .AsQueryable();

        }
    }
}

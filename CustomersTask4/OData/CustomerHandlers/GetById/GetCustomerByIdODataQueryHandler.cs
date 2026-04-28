using CustomersTask4.Data;
using CustomersTask4.DTO;
using CustomersTask4.Exceptions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Shared.Services;

namespace CustomersTask4.OData.CustomerHandlers.GetById
{
    public class GetCustomerByIdODataQueryHandler(
        ILogger<GetCustomerByIdODataQueryHandler>logger,
        IDbContextFactory<ApplicationDbContext> _dbFactory,
        ILocalizationService localization,
        IMapper mapper)
    {
        public async Task<CustomerDto?> Handle(GetCustomerByIdODataQuery request, CancellationToken cancellationToken)
        {
            var db=_dbFactory.CreateDbContext();
            logger.LogInformation($"Getting customer by id {request.id}");

            var customer =await db.Customers.Where(c=>c.Id==request.id).FirstOrDefaultAsync(cancellationToken);


            if (customer == null)
                    throw new NotFoundException(localization.Localize("CustomerNotFound",request.id));

            var res = mapper.Map<CustomerDto>(customer);
            return res;
        }
    }
}

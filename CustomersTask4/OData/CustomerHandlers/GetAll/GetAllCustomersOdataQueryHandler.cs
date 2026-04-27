using CustomersTask4.Data;
using CustomersTask4.DTO;
using Microsoft.EntityFrameworkCore;

namespace CustomersTask4.OData.CustomerHandlers.GetAll
{
    public class GetAllCustomersOdataQueryHandler(
       IDbContextFactory<ApplicationDbContext> _dbFactory,
        ILogger<GetAllCustomersOdataQueryHandler> logger
        )
    {
        public IQueryable<CustomerDto> Handle(
            GetAllCustomersOdataQuery request,
            CancellationToken cancellationToken)
        {
            var db = _dbFactory.CreateDbContext();
            logger.LogInformation("Get all customers OData query handler called.");

            return db.Customers
                   .AsNoTracking()
                   .Select(c => new CustomerDto
                   {
                       Id = c.Id,
                       Name = c.Name,
                       Phone= c.Phone,
                       CreatedAt = c.CreatedAt,
                       CreatedBy = c.CreatedBy,
                       ChangedAt = c.ChangedAt,
                       ChangedBy= c.ChangedBy,
                       Addresses = c.Addresses.Select(a => new AddressDto
                       {
                           AddressName = a.AddressName,
                           AddressType = a.AddressType.ToString()
                       }).ToList()
                   });





        }
    }
}

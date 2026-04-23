using CustomersTask4.Domain;
using CustomersTask4.DTO;
using CustomersTask4.Repository;
using CustomersTask4.Services;
using QuestPDF.Fluent;

namespace CustomersTask4.CQRS.CustomerHandler.Query.GenerateCustomerPDF
{
    public class GenerateCustomerPDFQueryHandler(IGenericRepository<Customer> repository,
        ILogger<GenerateCustomerPDFQueryHandler> logger)
    {
        public byte[] Handle(GenerateCustomerPDFQuery request,
          CancellationToken cancellationToken)
        {
            var customers = repository.
                GetAll(c=>DateOnly.FromDateTime(c.CreatedAt)>=request.From && DateOnly.FromDateTime(c.CreatedAt)<=request.To)
                .Select(c => new CustomerPDFDto()
                 {
                   Id = c.Id,
                   Name = c.Name,
                   Phone = c.Phone
                 }).ToList();
            logger.LogInformation($"Get Customers From {request.From} to {request.To}");
            var pdfService = new GenerateCustomerPDFService(customers, request.From, request.To);
            return pdfService.GeneratePdf();
        }
    }
}

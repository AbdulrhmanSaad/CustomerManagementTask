namespace CustomersTask4.CQRS.CustomerHandler.Query.GenerateCustomerPDF
{
    public class GenerateCustomerPDFQuery
    {
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }
    }
}

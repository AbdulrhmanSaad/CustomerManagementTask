namespace CustomersTask4.CQRS.CustomerHandler.Query.GetCustomerHistory
{
    public class GetCustomerHistoryQuery(string customerId)
    {
        public string CustomerId { get; } = customerId;
    }
}


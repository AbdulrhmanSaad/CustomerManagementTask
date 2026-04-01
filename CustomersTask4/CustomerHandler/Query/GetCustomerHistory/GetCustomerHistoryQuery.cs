namespace CustomersTask4.CustomerHandler.Query.GetCustomerHistory
{
    public class GetCustomerHistoryQuery(string customerId)
    {
        public string CustomerId { get; } = customerId;
    }
}


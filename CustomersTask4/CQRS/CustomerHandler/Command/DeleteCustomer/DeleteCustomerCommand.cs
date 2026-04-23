namespace CustomersTask4.CQRS.CustomerHandler.Command.DeleteCustomer
{
    public class DeleteCustomerCommand(string id)
    {
        public string Id { get; set; } = id;
    }
}



namespace CustomersTask4.CustomerHandler.Command.DeleteCustomerCommand
{
    public class DeleteCustomerCommand(string id)
    {
        public string Id { get; set; } = id;
    }
}

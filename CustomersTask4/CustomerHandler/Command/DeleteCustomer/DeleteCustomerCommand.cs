
using Mediator;

namespace CustomersTask4.CustomerHandler.Command.DeleteCustomerCommand
{
    public class DeleteCustomerCommand(string id): IRequest
    {
        public string Id { get; set; } = id;
    }
}

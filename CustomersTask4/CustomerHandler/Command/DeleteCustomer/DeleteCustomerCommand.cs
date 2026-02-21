using MediatR;

namespace CustomersTask4.CustomerHandler.Command.DeleteCustomerCommand
{
    public class DeleteCustomerCommand(int id): IRequest
    {
        public int Id { get; } = id;
    }
}

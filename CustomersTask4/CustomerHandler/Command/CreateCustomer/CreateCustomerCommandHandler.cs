using AutoMapper;
using CustomersTask4.Domain;
using CustomersTask4.Exceptions;
using CustomersTask4.Repository;
using CustomersTask4.Users;
using MediatR;

namespace CustomersTask4.CustomerHandler.Command.CreateCustomer
{
    public class CreateCustomerCommandHandler(IGenericRepository<Customer>db
        ,ILogger<CreateCustomerCommandHandler>logger
        ,IMapper mapper,
        IUserContext userContext) : IRequestHandler<CreateCustomerCommand>
    {
        public async Task Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creating a new customer with Data: {Data}", request);

            bool exist = db.PhoneExistsAsync(request.Phone);

            if (exist)
                throw new NotFoundException("this phone number already exists");

                
            var customer=mapper.Map<Customer>(request);

            var user = userContext.GetCurrentUser();
            if (user!=null)
                customer.CreatedBy=user.Name;

            await db.Add(customer);


        }

    }
}

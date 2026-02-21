using AutoMapper;
using Castle.Core.Resource;
using CustomersTask4.Domain;
using CustomersTask4.Exceptions;
using CustomersTask4.Repository;
using CustomersTask4.Services;
using MediatR;

namespace CustomersTask4.CustomerHandler.Command.UpdateCustomer
{
    public class UpdateCustomerCommandHandler(IGenericRepository<Customer>db,
        ILogger<UpdateCustomerCommandHandler>logger,
        IMapper mapper,
        IAuditService audit
        ) : IRequestHandler<UpdateCustomerCommand>
    {
        public async  Task Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await db.GetByIdAsync(request.Id,c=>c.Addresses);
            if (customer == null)
                throw new NotFoundException($"Customer with id {request.Id} not found.");
            if(db.PhoneExistsAsync(request.Phone))
                throw new NotFoundException($"Phone Number: {request.Phone} aleardy exists.");
            var old = mapper.Map<Customer>(customer);
            mapper.Map(request, customer);
            await db.Update(customer);
            await audit.LogCustomerChangeAsync(old, customer, "UPDATE");


        }
    }
}

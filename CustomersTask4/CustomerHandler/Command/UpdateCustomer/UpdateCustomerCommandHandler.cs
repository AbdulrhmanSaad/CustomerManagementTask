using AutoMapper;
using Castle.Core.Resource;
using CustomersTask4.Domain;
using CustomersTask4.Exceptions;
using CustomersTask4.Repository;
using CustomersTask4.Users;
using MediatR;

namespace CustomersTask4.CustomerHandler.Command.UpdateCustomer
{
    public class UpdateCustomerCommandHandler:IRequestHandler<UpdateCustomerCommand>
    {
        private IGenericRepository<Customer> db;
        private ILogger<UpdateCustomerCommandHandler> logger;
        private IMapper mapper;
        private IUserContext userContext;

        public UpdateCustomerCommandHandler(IGenericRepository<Customer> repository, ILogger<UpdateCustomerCommandHandler> logger, IMapper mapper,IUserContext userContext)
        {
            this.db = repository;
            this.logger = logger;
            this.mapper = mapper;
            this.userContext = userContext;
        }

        public async  Task Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
           
            var customer = await db.GetByIdAsync(request.Id,c=>c.Addresses);
           
            if (customer == null)
               throw new NotFoundException($"Customer with id {request.Id} not found.");
            
            if(db.PhoneExistsAsync(request.Phone)&&customer.Phone!=request.Phone)
                throw new NotFoundException($"Phone Number: {request.Phone} aleardy exists.");
            
            mapper.Map<Customer>(customer);
            mapper.Map(request, customer);
            var user=userContext.GetCurrentUser();
            if(user != null)
                customer.ChangedBy= user.Name;
            await db.Update(customer);


        }
    }
}

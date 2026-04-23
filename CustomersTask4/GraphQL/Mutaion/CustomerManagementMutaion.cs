using CustomersTask4.CQRS.CustomerHandler.Command.CreateCustomer;
using CustomersTask4.CQRS.CustomerHandler.Command.DeleteCustomer;
using CustomersTask4.CQRS.CustomerHandler.Command.UpdateCustomer;
using CustomersTask4.Domain;
using CustomersTask4.Repository;
using CustomersTask4.Users;
using FluentValidation;
using HotChocolate.Authorization;
using MapsterMapper;
using Shared.Services;

namespace CustomersTask4.GraphQL.Mutaion
{
    [Authorize]
    public class CustomerManagementMutaion
    {
        public async Task<string> CreateCustomer(
            CreateCustomerCommand request,
            IValidator<CreateCustomerCommand> validator,
            IGenericRepository<Customer> repo,
            IMapper mapper,
            IUserContext userContext,
            ILocalizationService localization)
        {
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => new IError[] { ErrorBuilder.New()
                        .SetMessage(e.ErrorMessage)
                        .SetExtension("field", e.PropertyName)
                        .Build() })
                    .SelectMany(e => e)
                    .ToList();

                throw new GraphQLException(errors);
            }
            //TODO: the phone number already exists true but deleted customer with the same phone number exists
            bool exist = repo.PhoneExistsAsync(request.Phone);
            if(exist)
            {
                throw new GraphQLException(localization.Localize("Phone number already exists"));
            }
            
            var customer=mapper.Map<Customer>(request);
            var user = userContext.GetCurrentUser();
            if (user != null)
                customer.CreatedBy = user.Name;
            await repo.Add(customer);
            return localization.Localize("Customer Created Successfully");
        }
         public async Task<string> UpdateCustomer(UpdateCustomerCommand request,
             IGenericRepository<Customer> repo,
             IMapper mapper,
             IUserContext userContext,
             ILocalizationService localization,
             IValidator<UpdateCustomerCommand> validator)
         {
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => new IError[] { ErrorBuilder.New()
                        .SetMessage(e.ErrorMessage)
                        .SetExtension("field", e.PropertyName)
                        .Build() })
                    .SelectMany(e => e)
                    .ToList();

                throw new GraphQLException(errors);
            }
            var customer = await repo.GetByIdAsync(request.Id, c => c.Addresses);

            if (customer == null)
                throw new GraphQLException(localization.Localize($"Customer with id {request.Id} not found."));

            if (repo.PhoneExistsAsync(request.Phone) && customer.Phone != request.Phone)
                throw new GraphQLException(localization.Localize("Customer Phone Exist", request.Phone));

            mapper.Map(request, customer);

            var user = userContext.GetCurrentUser();
            if (user != null)
                customer.ChangedBy = user.Name;

            customer.ChangedAt = DateTime.UtcNow;

            await repo.Update(customer);
            return localization.Localize("Customer Updated");
         }
         public async Task<string> DeleteCustomer(DeleteCustomerCommand request,
             IGenericRepository<Customer> repo,
             IMapper mapper,
             IUserContext userContext,
             ILocalizationService localization)
         {
            var customer = await repo.GetByIdAsync(request.Id);
            if (customer == null)
                throw new GraphQLException(localization.Localize("CustomerNotFound", request.Id));

            await repo.Delete(customer);
            return localization.Localize("Customer Deleted Successfully");
         }
    }
}

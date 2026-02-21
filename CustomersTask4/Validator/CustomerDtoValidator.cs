using CustomersTask4.CustomerHandler.Command.CreateCustomer;
using CustomersTask4.DTO;
using FluentValidation;

namespace CustomersTask4.Validator
{
    public class CustomerDtoValidator:AbstractValidator<CreateCustomerCommand>
    {
        public CustomerDtoValidator()
        {
         RuleFor(c=>c.Name).NotEmpty().WithMessage("Name is required.")
             .MaximumLength(200).WithMessage("Name must not exceed 100 characters.");

         RuleFor(c=>c.Phone)
                    .NotEmpty()
                    .Matches(@"^(010|011|012|015)\d{8}$")
                    .WithMessage("Invalid Egyptian phone number");

        }
    }
}

using CustomersTask4.CustomerHandler.Command.CreateCustomer;
using CustomersTask4.Domain;
using CustomersTask4.DTO;
using FluentValidation;

namespace CustomersTask4.Validator
{
    public class CreateCustomerCommandValidator:AbstractValidator<CreateCustomerCommand>
    {
        public CreateCustomerCommandValidator()
        {
         RuleFor(c=>c.Name).NotEmpty().WithMessage("Name is required.")
             .MaximumLength(200).WithMessage("Name must not exceed 100 characters.");

         RuleFor(c=>c.Phone)
                    .NotEmpty()
                    .Matches(@"^(010|011|012|015)\d{8}$")
                    .WithMessage("Invalid Egyptian phone number");

            RuleFor(x => x.AddressType)
                .IsInEnum().WithMessage("AddressType must be valid.");

            RuleFor(x => x.HomeAddressLocation)
                .NotEmpty().When(x => x.AddressType == AddressType.Home)
                .WithMessage("Home address is required when AddressType is Home.");

            RuleFor(x => x.AddressType2)
                .IsInEnum().WithMessage("AddressType2 must be valid.");

            RuleFor(x => x.WorkAddressLocation)
                .NotEmpty().When(x => x.AddressType2 == AddressType.Work)
                .WithMessage("Work address is required when AddressType2 is Work.");

            RuleFor(x => x)
              .Must(x => x.AddressType != x.AddressType2)
              .WithMessage("Address types must be different.");



        }
    }
}

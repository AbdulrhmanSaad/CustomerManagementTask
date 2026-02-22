using CustomersTask4.CustomerHandler.Command.UpdateCustomer;
using CustomersTask4.Domain;
using FluentValidation;

namespace CustomersTask4.Validator
{
    public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
    {
        public UpdateCustomerCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id must be greater than 0.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(150).WithMessage("Name must not exceed 150 characters.");

            RuleFor(c => c.Phone)
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

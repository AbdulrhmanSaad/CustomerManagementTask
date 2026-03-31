using CustomersTask4.CustomerHandler.Command.UpdateCustomer;
using CustomersTask4.Domain;
using CustomersTask4.Services;
using FluentValidation;

namespace CustomersTask4.Validator
{
    public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
    {
        private readonly ILocalizationService localization;

        public UpdateCustomerCommandValidator(ILocalizationService localization)
        {
            this.localization=localization;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(localization.Localize("Name is required."))
                .MaximumLength(150).WithMessage(localization.Localize("Name must not exceed 150 characters."));

            RuleFor(c => c.Phone)
                   .NotEmpty()
                   .Matches(@"^(010|011|012|015)\d{8}$")
                   .WithMessage(localization.Localize("Invalid Egyptian phone number"));

            RuleFor(x => x.Addresses)
               .NotEmpty()
               .WithMessage(localization.Localize("Addresses cannot be null."))
               .Must(addresses => addresses != null)
               .WithMessage(localization.Localize("Addresses cannot be null."));

            RuleForEach(x => x.Addresses)
                .ChildRules(address =>
                {
                    address.RuleFor(a => a.AddressName)
                        .NotEmpty()
                        .WithMessage(localization.Localize("Address name/location is required."))
                        .MaximumLength(500)
                        .WithMessage(localization.Localize("Address name must not exceed 500 characters."))
                        .MinimumLength(3)
                        .WithMessage(localization.Localize("Address name must be at least 3 characters long."));

                    address.RuleFor(a => a.AddressType)
                        .IsInEnum()
                        .WithMessage("Invalid address type. Must be Home (0) or Work (1).");

                });
            RuleFor(x => x.Addresses)
               .Must(addresses => addresses.Count <= 2)
               .WithMessage(localization.Localize("Maximum of 2 addresses allowed."));


            RuleFor(x => x.Addresses)
               .Must(addresses =>
               {
                   if (addresses == null || addresses.Count == 0)
                       return true;

                   var addressTypes = addresses.Select(a => a.AddressType).ToList();
                   var uniqueTypes = addressTypes.Distinct().Count();

                   return uniqueTypes == addressTypes.Count;
               })
               .WithMessage(localization.Localize("Cannot have duplicate address types. Each address must have a unique type."));

        }
    }
}

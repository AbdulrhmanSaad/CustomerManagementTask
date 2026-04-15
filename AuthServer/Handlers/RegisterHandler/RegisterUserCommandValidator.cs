using FluentValidation;
using Shared.Services;

namespace AuthServer.Handlers.RegisterHandler
{
    public class RegisterUserCommandValidator:AbstractValidator<RegisterUserCommand>
    {
        private readonly ILocalizationService localization;
        public RegisterUserCommandValidator(ILocalizationService _loca)
        {
            localization = _loca;
            RuleFor(x => x.Email)
              .NotEmpty().WithMessage(localization.Localize("Email is required."))
              .EmailAddress().WithMessage("Invalid email format.");
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage(localization.Localize("Name is required"))
                .MinimumLength(3).WithMessage(localization.Localize("User Name must be at least 3 characters."));
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(localization.Localize("Password is required."))
                .MinimumLength(8).WithMessage(localization.Localize("Password must be at least 8 characters."))
                .Matches("[A-Z]").WithMessage(localization.Localize("Password must contain at least one uppercase letter."))
                .Matches("[a-z]").WithMessage(localization.Localize("Password must contain at least one lowercase letter."))
                .Matches("[0-9]").WithMessage(localization.Localize("Password must contain at least one number."));

        }
    }
}

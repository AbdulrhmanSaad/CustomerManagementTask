using FluentValidation;
namespace CustomersTask4.UserHandler.Command.RegisterNewUser
{
    public class RegisterNewUserCommandValidator : AbstractValidator<RegisterNewUserCommand>
    {
        public RegisterNewUserCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(7).WithMessage("Password must be at least 7 characters.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one number.");
        }
    }
}

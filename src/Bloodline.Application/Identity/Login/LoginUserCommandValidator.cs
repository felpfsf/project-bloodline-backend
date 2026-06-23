using FluentValidation;

namespace Bloodline.Application.Identity.Login;

public sealed class LoginUserCommandValidator : AbstractValidator<LoginCommand>
{
  public LoginUserCommandValidator()
  {
    RuleFor(command => command.Email)
        .NotEmpty()
        .WithErrorCode(LoginUserErrorCodes.EmailRequired)
        .WithMessage("Email is required.")
        .MaximumLength(320)
        .WithErrorCode(LoginUserErrorCodes.EmailMaxLength)
        .WithMessage("Email must have at most 320 characters.")
        .EmailAddress()
        .WithErrorCode(LoginUserErrorCodes.EmailInvalid)
        .WithMessage("Email format is invalid.");

    RuleFor(command => command.Password)
        .NotEmpty()
        .WithErrorCode(LoginUserErrorCodes.PasswordRequired)
        .WithMessage("Password is required.")
        .MaximumLength(128)
        .WithErrorCode(LoginUserErrorCodes.PasswordMaxLength)
        .WithMessage("Password must have at most 128 characters.");
  }
}
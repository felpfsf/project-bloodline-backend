using FluentValidation;

namespace Bloodline.Application.Identity.Register;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
  public RegisterUserCommandValidator()
  {
    RuleFor(command => command.Email)
        .NotEmpty()
        .WithErrorCode(RegisterUserErrorCodes.EmailRequired)
        .WithMessage("Email is required.")
        .MaximumLength(320)
        .WithErrorCode(RegisterUserErrorCodes.EmailMaxLength)
        .WithMessage("Email must have at most 320 characters.")
        .EmailAddress()
        .WithErrorCode(RegisterUserErrorCodes.EmailInvalid)
        .WithMessage("Email format is invalid.");

    RuleFor(command => command.Password)
        .NotEmpty()
        .WithErrorCode(RegisterUserErrorCodes.PasswordRequired)
        .WithMessage("Password is required.")
        .MinimumLength(8)
        .WithErrorCode(RegisterUserErrorCodes.PasswordMinLength)
        .WithMessage("Password must be at least 8 characters long.")
        .MaximumLength(128)
        .WithErrorCode(RegisterUserErrorCodes.PasswordMaxLength)
        .WithMessage("Password must have at most 128 characters.");
  }
}
using Bloodline.Application.Common.Validation;
using Bloodline.Application.Identity.Abstractions;
using FluentValidation;

namespace Bloodline.Application.Identity.Login;

public sealed class LoginUserHandler
{
  private readonly IUserRepository _userRepository;
  private readonly IPasswordHasher _passwordHasher;
  private readonly IValidator<LoginCommand> _validator;

  public LoginUserHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IValidator<LoginCommand> validator
  )
  {
    _userRepository = userRepository;
    _passwordHasher = passwordHasher;
    _validator = validator;
  }

  public async Task<LoginUserResult> HandleAsync(LoginCommand command, CancellationToken cancellationToken)
  {
    var validationResult = await _validator.ValidateAsync(command, cancellationToken);

    if (!validationResult.IsValid)
    {
      var errors = validationResult.Errors.Select(
        error => new ValidationError(
          error.PropertyName,
          error.ErrorCode,
          error.ErrorMessage
        )
      ).ToArray();

      return LoginUserResult.ValidationFailure(errors);
    }

    var normalizedEmail = command.Email.Trim().ToUpperInvariant();

    var user = await _userRepository.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken);

    if (user is null)
    {
      return LoginUserResult.Failure(LoginUserError.InvalidCredentials);
    }

    var isPasswordValid = _passwordHasher.VerifyHashedPassword(user.PasswordHash, command.Password);

    if (!isPasswordValid)
    {
      return LoginUserResult.Failure(LoginUserError.InvalidCredentials);
    }

  return LoginUserResult.Success(user.Id, user.Email);
  }
}
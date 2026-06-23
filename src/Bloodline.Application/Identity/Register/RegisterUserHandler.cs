
using Bloodline.Application.Common.Persistence;
using Bloodline.Application.Common.Time;
using Bloodline.Application.Common.Validation;
using Bloodline.Application.Identity.Abstractions;
using Bloodline.Domain.Identity;
using FluentValidation;

namespace Bloodline.Application.Identity.Register;

public sealed class RegisterUserHandler
{
  private readonly IUserRepository _userRepository;
  private readonly IPasswordHasher _passwordHasher;
  private readonly IClock _clock;
  private readonly IValidator<RegisterUserCommand> _validator;

  public RegisterUserHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IClock clock,
    IValidator<RegisterUserCommand> validator
  )
  {
    _userRepository = userRepository;
    _passwordHasher = passwordHasher;
    _clock = clock;
    _validator = validator;
  }

  public async Task<RegisterUserResult> HandleAsync(
    RegisterUserCommand command,
    CancellationToken cancellationToken
  )
  {
    var validationResult = await _validator.ValidateAsync(command, cancellationToken);

    if (!validationResult.IsValid)
    {
      var errors = validationResult.Errors.Select(error => new ValidationError(
        error.PropertyName,
        error.ErrorCode,
        error.ErrorMessage
      )).ToArray();

      return RegisterUserResult.ValidationFailure(errors);
    }


    var email = command.Email.Trim();
    var normalizedEmail = email.ToUpperInvariant();

    var userAlreadyExists = await _userRepository.ExistsByNormalizedEmailAsync(normalizedEmail, cancellationToken);

    if (userAlreadyExists)
    {
      return RegisterUserResult.Failure(RegisterUserError.EmailAlreadyExists);
    }

    var passwordHash = _passwordHasher.HashPassword(command.Password);

    var user = User.Create(
      Guid.NewGuid(),
      email,
      normalizedEmail,
      passwordHash,
      _clock.UtcNow
    );

    try
    {
      await _userRepository.CreateAsync(user, cancellationToken);
    }
    catch (UniqueConstraintViolationException exception)
      when (exception.ConstraintName == "ux_users_normalized_email")
    {
      return RegisterUserResult.Failure(RegisterUserError.EmailAlreadyExists);
    }

    return RegisterUserResult.Success(
      user.Id,
      user.Email,
      user.CreatedAtUtc
    );
  }
}
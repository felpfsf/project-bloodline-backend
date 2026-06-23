using Bloodline.Application.Common.Validation;

namespace Bloodline.Application.Identity.Register;

public sealed record RegisterUserResult
{
  private RegisterUserResult(
    Guid? userid,
    string? email,
    DateTimeOffset? createdatutc,
    RegisterUserError? error,
    IReadOnlyCollection<ValidationError> validationErrors
  )
  {
    UserId = userid;
    Email = email;
    CreatedAtUtc = createdatutc;
    Error = error;
    ValidationErrors = validationErrors;
  }

  public Guid? UserId { get; }
  public string? Email { get; }
  public DateTimeOffset? CreatedAtUtc { get; }
  public RegisterUserError? Error { get; }
  public IReadOnlyCollection<ValidationError> ValidationErrors { get; }

  public bool IsSuccess => Error is null;

  public static RegisterUserResult Success(
    Guid userid,
    string email,
    DateTimeOffset createdAtUtc
  )
  {
    return new RegisterUserResult(
      userid,
      email,
      createdAtUtc,
      null,
      // Array.Empty<ValidationError>()
      []
    );
  }

  public static RegisterUserResult Failure(RegisterUserError error)
  {
    return new RegisterUserResult(
      null,
      null,
      null,
      error,
      // Array.Empty<ValidationError>()
      []
    );
  }

  public static RegisterUserResult ValidationFailure(
    IReadOnlyCollection<ValidationError> validationErrors
  )
  {
    return new RegisterUserResult(
      null,
      null,
      null,
      RegisterUserError.ValidationFailed,
      validationErrors
    );
  }

}
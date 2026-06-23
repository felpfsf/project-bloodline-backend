using Bloodline.Application.Common.Validation;

namespace Bloodline.Application.Identity.Login;

public sealed record LoginUserResult
{
  private LoginUserResult(
    Guid? userId,
    string? email,
    LoginUserError? error,
    IReadOnlyCollection<ValidationError> validationErrors
  )
  {
    UserId = userId;
    Email = email;
    Error = error;
    ValidationErrors = validationErrors;
  }

  public Guid? UserId { get; }
  public string? Email { get; }
  public LoginUserError? Error { get; }
  public IReadOnlyCollection<ValidationError> ValidationErrors { get; }
  public bool IsSuccess => Error is null && ValidationErrors.Count == 0;

  public static LoginUserResult Success(Guid userId, string email)
  {
    return new LoginUserResult(
      userId,
      email,
      null,
      validationErrors: []
    );
  }

  public static LoginUserResult Failure(LoginUserError error)
  {
    return new LoginUserResult(
      null,
      null,
      error,
      validationErrors: []
    );
  }

  public static LoginUserResult ValidationFailure(IReadOnlyCollection<ValidationError> validationErrors)
  {
    return new LoginUserResult(
      null,
      null,
      error: LoginUserError.ValidationFailed,
      validationErrors
    );
  }



}
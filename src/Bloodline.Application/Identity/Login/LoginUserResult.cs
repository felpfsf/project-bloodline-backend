using Bloodline.Application.Common.Validation;
using Bloodline.Application.Identity.Models;

namespace Bloodline.Application.Identity.Login;

public sealed record LoginUserResult
{
  private LoginUserResult(
    Guid? userId,
    string? email,
    AccessToken? accessToken,
    LoginUserError? error,
    IReadOnlyCollection<ValidationError> validationErrors
  )
  {
    UserId = userId;
    Email = email;
    AccessToken = accessToken;
    Error = error;
    ValidationErrors = validationErrors;
  }

  public Guid? UserId { get; }
  public string? Email { get; }
  public AccessToken? AccessToken { get; }
  public LoginUserError? Error { get; }
  public IReadOnlyCollection<ValidationError> ValidationErrors { get; }
  public bool IsSuccess => Error is null && ValidationErrors.Count == 0;

  public static LoginUserResult Success(Guid userId, string email, AccessToken accessToken)
  {
    return new LoginUserResult(
      userId,
      email,
      accessToken,
      error: null,
      validationErrors: []
    );
  }

  public static LoginUserResult Failure(LoginUserError error)
  {
    return new LoginUserResult(
      null,
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
      null,
      error: LoginUserError.ValidationFailed,
      validationErrors
    );
  }



}
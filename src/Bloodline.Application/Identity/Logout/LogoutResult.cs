namespace Bloodline.Application.Identity.Logout;

public sealed record LogoutResult
{
  private LogoutResult(bool isSuccess) => IsSuccess = isSuccess;

  public bool IsSuccess { get; }

  public static LogoutResult Success() => new(true);
}

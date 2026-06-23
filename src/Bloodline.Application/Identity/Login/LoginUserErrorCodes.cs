namespace Bloodline.Application.Identity.Login;

public static class LoginUserErrorCodes
{
  public const string EmailRequired = "email_required";
  public const string EmailMaxLength = "email_max_length";
  public const string EmailInvalid = "email_invalid";
  public const string PasswordRequired = "password_required";
  public const string PasswordMaxLength = "password_max_length";
}
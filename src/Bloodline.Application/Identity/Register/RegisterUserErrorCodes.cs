namespace Bloodline.Application.Identity.Register;

public static class RegisterUserErrorCodes
{
  public const string EmailRequired = "email_required";
  public const string EmailMaxLength = "email_max_length";
  public const string EmailInvalid = "email_invalid";

  public const string PasswordRequired = "password_required";
  public const string PasswordMinLength = "password_min_length";
  public const string PasswordMaxLength = "password_max_length";
}

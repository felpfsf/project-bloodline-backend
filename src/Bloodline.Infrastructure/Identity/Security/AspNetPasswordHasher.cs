using Bloodline.Application.Identity.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace Bloodline.Infrastructure.Identity.Security;

public sealed class AspNetPasswordHasher : IPasswordHasher
{
  private readonly PasswordHasher<object> _passwordHasher = new();
  public string HashPassword(string password)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(password, nameof(password));

    return _passwordHasher.HashPassword(user: new object(), password);
  }

  public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(hashedPassword, nameof(hashedPassword));
    ArgumentException.ThrowIfNullOrWhiteSpace(providedPassword, nameof(providedPassword));

    var result = _passwordHasher.VerifyHashedPassword(user: new object(), hashedPassword, providedPassword);

    return result is
      PasswordVerificationResult.Success or
      PasswordVerificationResult.SuccessRehashNeeded;
  }
}
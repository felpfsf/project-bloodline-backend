using Bloodline.Application.Identity.Abstractions;

namespace Bloodline.Application.Tests.Identity.Fakes;

internal sealed class FakePasswordHasher : IPasswordHasher
{
  public string HashToReturn { get; set; } = "generated-hash";
  public string? ReceivedPassword { get; private set; }
  public bool VerificationResult { get; set; }
  public string? ReceivedHashedPassword { get; private set; }
  public string? ReceivedProvidedPassword { get; private set; }

  public string HashPassword(string password)
  {
    ReceivedPassword = password;

    return HashToReturn;
  }

  public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
  {
    ReceivedHashedPassword = hashedPassword;
    ReceivedProvidedPassword = providedPassword;

    return VerificationResult;
  }
}
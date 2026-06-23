using Bloodline.Application.Common.Persistence;
using Bloodline.Application.Identity.Abstractions;
using Bloodline.Domain.Identity;

namespace Bloodline.Application.Tests.Identity.Register.Fakes;

internal sealed class FakeUserRepository : IUserRepository
{
  public bool UserExists { get; set; }
  public bool ThrowUniqueConstraintViolationException { get; set; }
  public string? CheckedNormalizedEmail { get; private set; }
  public User? CreatedUser { get; private set; }

  public Task<bool> ExistsByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
  {
    CheckedNormalizedEmail = normalizedEmail;

    return Task.FromResult(UserExists);
  }

  public Task CreateAsync(User user, CancellationToken cancellationToken)
  {
    if (ThrowUniqueConstraintViolationException)
    {
      throw new UniqueConstraintViolationException(
        "ux_users_normalized_email",
        new Exception("Unique constraint violation exception for testing purposes")
      );
    }

    CreatedUser = user;

    return Task.CompletedTask;
  }
}
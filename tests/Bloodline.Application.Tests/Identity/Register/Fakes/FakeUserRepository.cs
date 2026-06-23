using Bloodline.Application.Identity.Abstractions;
using Bloodline.Domain.Identity;

namespace Bloodline.Application.Tests.Identity.Register.Fakes;

internal sealed class FakeUserRepository : IUserRepository
{
  public bool UserExists { get; set; } = false;

  public string? CheckedNormalizedEmail { get; private set; }

  public User? CreatedUser { get; private set; }

  public Task<bool> ExistsByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
  {
    CheckedNormalizedEmail = normalizedEmail;

    return Task.FromResult(UserExists);
  }

  public Task CreateAsync(User user, CancellationToken cancellationToken)
  {
    CreatedUser = user;

    return Task.CompletedTask;
  }
}
using Bloodline.Domain.Identity;

namespace Bloodline.Application.Identity.Abstractions;

public interface IUserRepository
{
  Task<bool> ExistsByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken);
  Task CreateAsync(User user, CancellationToken cancellationToken);
}
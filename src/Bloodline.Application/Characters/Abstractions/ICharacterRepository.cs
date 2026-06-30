using Bloodline.Domain.Characters;

namespace Bloodline.Application.Characters.Abstractions;

public interface ICharacterRepository
{
  Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken);
  Task<bool> ExistsByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken);
  Task CreateAsync(Character character, CancellationToken cancellationToken);
}
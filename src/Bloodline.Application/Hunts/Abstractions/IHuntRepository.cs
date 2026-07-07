
using Bloodline.Domain.Hunts;

namespace Bloodline.Application.Hunts.Abstractions;

public interface IHuntRepository
{
  Task<bool> ExistsByCharacterIdAsync(Guid characterId, CancellationToken cancellationToken);
  Task<Hunt?> GetActiveByCharacterIdAsync(Guid characterId, CancellationToken cancellationToken);
  Task<Hunt?> GetByIdAndCharacterIdAsync(Guid huntId, Guid characterId, CancellationToken cancellationToken);
  Task CreateAsync(Hunt hunt, CancellationToken cancellationToken);
}
using Bloodline.Application.Hunts.Abstractions;
using Bloodline.Domain.Hunts;

namespace Bloodline.Application.Tests.Hunts.Fakes;

public sealed class FakeHuntRepository : IHuntRepository
{
  public bool HasActiveHunt { get; set; }
  public Hunt? ActiveHuntToReturn { get; set; }
  public Hunt? HuntToReturn { get; set; }
  public Guid? CheckedActiveCharacterId { get; private set; }
  public Guid? RequestedActiveCharacterId { get; private set; }
  public Guid? RequestedHuntId { get; private set; }
  public Guid? RequestedHuntCharacterId { get; private set; }
  public Hunt? CreatedHunt { get; private set; }

  public Task CreateAsync(Hunt hunt, CancellationToken cancellationToken)
  {
    CreatedHunt = hunt;
    return Task.CompletedTask;
  }

  public Task<bool> ExistsByCharacterIdAsync(Guid characterId, CancellationToken cancellationToken)
  {
    CheckedActiveCharacterId = characterId;
    return Task.FromResult(HasActiveHunt);
  }

  public Task<Hunt?> GetActiveByCharacterIdAsync(Guid characterId, CancellationToken cancellationToken)
  {
    RequestedActiveCharacterId = characterId;
    return Task.FromResult(ActiveHuntToReturn);
  }

  public Task<Hunt?> GetByIdAndCharacterIdAsync(Guid huntId, Guid characterId, CancellationToken cancellationToken)
  {
    RequestedHuntId = huntId;
    return Task.FromResult(HuntToReturn);
  }
}
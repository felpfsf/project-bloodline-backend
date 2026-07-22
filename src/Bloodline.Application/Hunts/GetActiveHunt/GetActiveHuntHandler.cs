using Bloodline.Application.Characters.Abstractions;
using Bloodline.Application.Hunts.Abstractions;

namespace Bloodline.Application.Hunts.GetActiveHunt;

public sealed class GetActiveHuntHandler(
  ICharacterRepository characterRepository,
  IHuntRepository huntRepository
)
{
  private readonly ICharacterRepository _characterRepository = characterRepository;
  private readonly IHuntRepository _huntRepository = huntRepository;


  public async Task<GetActiveHuntResult> HandleAsync(
    GetActiveHuntQuery query,
    CancellationToken cancellationToken
  )
  {
    var character = await _characterRepository.GetByUserIdAsync(
      query.UserId,
      cancellationToken
    );

    if (character is null)
    {
      return GetActiveHuntResult.Failure(GetActiveHuntError.CharacterNotFound);
    }

    var hunt = await _huntRepository.GetActiveByCharacterIdAsync(
      character.Id,
      cancellationToken
    );

    if (hunt is null)
    {
      return GetActiveHuntResult.NotActiveHunt();
    }

    return GetActiveHuntResult.Success(
      hunt.Id,
      hunt.DefinitionCode,
      hunt.StartsAtUtc,
      hunt.EndsAtUtc,
      hunt.EnergyCost,
      hunt.ExperienceReward,
      hunt.GoldReward
    );
  }

}
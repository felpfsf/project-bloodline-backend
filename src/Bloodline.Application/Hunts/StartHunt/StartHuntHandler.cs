using Bloodline.Application.Characters.Abstractions;
using Bloodline.Application.Common.Persistence;
using Bloodline.Application.Common.Time;
using Bloodline.Application.Hunts.Abstractions;
using Bloodline.Domain.Hunts;

namespace Bloodline.Application.Hunts.StartHunt;

public sealed class StartHuntHandler(
  ICharacterRepository characterRepository,
  IHuntRepository huntRepository,
  IUnitOfWork unitOfWork,
  IClock clock
)
{
  private readonly ICharacterRepository _characterRepository = characterRepository;
  private readonly IHuntRepository _huntRepository = huntRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;
  private readonly IClock _clock = clock;

  public async Task<StartHuntResult> HandleAsync(StartHuntCommand command, CancellationToken cancellationToken)
  {
    var character = await _characterRepository.GetByUserIdForUpdateAsync(command.UserId, cancellationToken);

    if (character is null)
    {
      return StartHuntResult.Failure(StartHuntError.CharacterNotFound);
    }

    var hasActiveHunt = await _huntRepository.ExistsByCharacterIdAsync(character.Id, cancellationToken);

    if (hasActiveHunt)
    {
      return StartHuntResult.Failure(StartHuntError.HuntAlreadyActive);
    }

    var definition = HuntDefinitions.ShadowHunt;

    if (!character.HasEnoughEnergy(definition.EnergyCost))
    {
      return StartHuntResult.Failure(StartHuntError.NotEnoughEnergy);
    }

    var nowUtc = _clock.UtcNow;
    character.ConsumeEnergy(definition.EnergyCost);

    var hunt = Hunt.Start(
      characterId: character.Id,
      definitionCode: definition.Code,
      startsAtUtc: nowUtc,
      duration: definition.Duration,
      energyCost: definition.EnergyCost,
      experienceReward: definition.ExperienceReward,
      goldReward: definition.GoldReward
    );

    await _huntRepository.CreateAsync(hunt, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return StartHuntResult.Success(
      huntId: hunt.Id,
      definitionCode: hunt.DefinitionCode,
      startsAtUtc: hunt.StartsAtUtc,
      endsAtUtc: hunt.EndsAtUtc,
      energyCost: hunt.EnergyCost,
      experienceReward: hunt.ExperienceReward,
      goldReward: hunt.GoldReward,
      remainingEnergy: character.Energy
    );
  }
}

using Bloodline.Application.Characters.Abstractions;
using Bloodline.Application.Common.Persistence;
using Bloodline.Application.Common.Time;
using Bloodline.Application.Hunts.Abstractions;
using Bloodline.Domain.Hunts;

namespace Bloodline.Application.Hunts.CompleteHunt;

public sealed class CompleteHuntHandler(
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

  public async Task<CompleteHuntResult> HandleAsync(
    CompleteHuntCommand command,
    CancellationToken cancellationToken
  )
  {
    var character = await _characterRepository.GetByUserIdForUpdateAsync(
      command.UserId,
      cancellationToken
    );

    if (character is null)
      return CompleteHuntResult.Failure(CompleteHuntError.CharacterNotFound);

    var hunt = await _huntRepository.GetByIdAndCharacterIdAsync(
      command.HuntId,
      character.Id,
      cancellationToken
    );

    if (hunt is null)
      return CompleteHuntResult.Failure(CompleteHuntError.HuntNotFound);

    if (hunt.Status == HuntStatus.Completed)
      return BuildSuccessResult(hunt);

    var nowUtc = _clock.UtcNow;

    if (!hunt.CanCompleteAt(nowUtc))
      return CompleteHuntResult.Failure(CompleteHuntError.HuntNotFinishedYet);

    hunt.Complete(nowUtc);

    character.ReceiveHuntRewards(
      hunt.ExperienceReward,
      hunt.GoldReward
    );

    try
    {
      await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
    catch (ConcurrencyConflictException)
    {
      var persistedHunt = await _huntRepository.GetSnapshotByIdAndCharacterIdAsync(
        hunt.Id,
        character.Id,
        cancellationToken
      );

      if (
        persistedHunt is not null &&
        persistedHunt.Status == HuntStatus.Completed &&
        persistedHunt.CompletedAtUtc.HasValue
      )
      {
        return BuildSuccessResult(hunt);
      }
      throw;
    }

    return BuildSuccessResult(hunt);
  }

  private static CompleteHuntResult BuildSuccessResult(Hunt hunt)
  {
    if (!hunt.CompletedAtUtc.HasValue)
    {
      throw new InvalidOperationException("A completed hunt must have a completion timestamp.");
    }

    return CompleteHuntResult.Success(
      hunt.Id,
      hunt.DefinitionCode,
      hunt.CompletedAtUtc.Value,
      hunt.ExperienceReward,
      hunt.GoldReward
    );
  }
}
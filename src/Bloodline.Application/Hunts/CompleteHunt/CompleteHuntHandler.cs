using Bloodline.Application.Abstractions.Events;
using Bloodline.Application.Characters.Abstractions;
using Bloodline.Application.Common.Persistence;
using Bloodline.Application.Common.Time;
using Bloodline.Application.Hunts.Abstractions;
using Bloodline.Domain.Hunts;
using Microsoft.Extensions.Logging;

namespace Bloodline.Application.Hunts.CompleteHunt;

public sealed class CompleteHuntHandler(
  ICharacterRepository characterRepository,
  IHuntRepository huntRepository,
  IUnitOfWork unitOfWork,
  IClock clock,
  IDomainEventDispatcher eventDispatcher,
  ILogger<CompleteHuntHandler> logger
)
{
  private readonly ICharacterRepository _characterRepository = characterRepository;
  private readonly IHuntRepository _huntRepository = huntRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;
  private readonly IClock _clock = clock;
  private readonly IDomainEventDispatcher _eventDispatcher = eventDispatcher;
  private readonly ILogger<CompleteHuntHandler> _logger = logger;

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

    character.ReceiveExperience(hunt.ExperienceReward);
    character.ReceiveGold(hunt.GoldReward);

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
        return BuildSuccessResult(persistedHunt);
      }
      throw;
    }

    var pendingEvents = character.DomainEvents.ToArray();

    try
    {
      foreach (var domainEvent in pendingEvents)
      {
        try
        {
          await _eventDispatcher.DispatchAsync(
            domainEvent,
            CancellationToken.None
          );
        }
        catch (Exception exception)
        {
          _logger.LogError(
            exception,
            "Failed to dispatch {DomainEventType} for Character {CharacterId}",
            domainEvent.GetType().Name,
            character.Id
          );
          throw;
        }
      }
    }
    finally
    {
      character.ClearDomainEvents();
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
using Bloodline.Application.Abstractions.Events;
using Bloodline.Domain.Characters.Events;
using Microsoft.Extensions.Logging;

namespace Bloodline.Application.Characters.EventHandlers;

public class LogCharacterLeveledUpHandler(ILogger<LogCharacterLeveledUpHandler> logger) : IDomainEventHandler<CharacterLeveledUp>
{
  private readonly ILogger<LogCharacterLeveledUpHandler> _logger = logger;

  public Task HandleAsync(
    CharacterLeveledUp domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    _logger.LogInformation(
      "Character {CharacterId} leveled up from {PreviousLevel} to level {NewLevel}. Total experience is now {TotalExperience}.",
      domainEvent.CharacterId,
      domainEvent.PreviousLevel,
      domainEvent.NewLevel,
      domainEvent.TotalExperience
    );

    return Task.CompletedTask;
  }

}
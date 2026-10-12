using Bloodline.Application.Abstractions.Events;
using Bloodline.Domain.Characters.Events;
using Microsoft.Extensions.Logging;

namespace Bloodline.Application.Characters.EventHandlers;

public class LogCharacterLevelUpHandler(ILogger<LogCharacterLevelUpHandler> logger) : IDomainEventHandler<CharacterLevelUp>
{
  private readonly ILogger<LogCharacterLevelUpHandler> _logger = logger;

  public Task HandleAsync(
    CharacterLevelUp domainEvent,
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
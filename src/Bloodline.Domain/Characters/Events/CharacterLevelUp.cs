using Bloodline.Domain.Common;

namespace Bloodline.Domain.Characters.Events;

public sealed record CharacterLevelUp(
  Guid CharacterId,
  int NewLevel,
  int PreviousLevel,
  long TotalExperience
) : IDomainEvent;
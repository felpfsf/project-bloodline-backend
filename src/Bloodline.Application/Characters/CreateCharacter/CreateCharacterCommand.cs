using Bloodline.Domain.Characters;

namespace Bloodline.Application.Characters.CreateCharacter;

public sealed record CreateCharacterCommand(
  Guid UserId,
  string Name,
  Faction Faction
);
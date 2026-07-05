namespace Bloodline.Api.Features.Characters.GetCurrentCharacter;

public sealed record GetCurrentCharacterResponse(
  Guid Id,
  string Name,
  string Faction,
  int Level,
  long Experience,
  long Gold,
  int Energy,
  int MaxEnergy,
  int Strength,
  int Defense,
  int Agility,
  DateTimeOffset CreatedAtUtc
);
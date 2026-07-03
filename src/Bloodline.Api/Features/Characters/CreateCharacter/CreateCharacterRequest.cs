namespace Bloodline.Api.Features.Characters.CreateCharacter;

public sealed record CreateCharacterRequest(
  string Name,
  string Faction
);
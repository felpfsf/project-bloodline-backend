using Bloodline.Domain.Characters;

namespace Bloodline.Application.Characters.GetCurrentCharacter;

public sealed record GetCurrentCharacterResult
{
  private GetCurrentCharacterResult(
    Guid? id,
    string? name,
    Faction? faction,
    int? level,
    long? experience,
    long? gold,
    int? energy,
    int? maxEnergy,
    int? strength,
    int? defense,
    int? agility,
    DateTimeOffset? createdAtUtc
  )
  {
    Id = id;
    Name = name;
    Faction = faction;
    Level = level;
    Experience = experience;
    Gold = gold;
    Energy = energy;
    MaxEnergy = maxEnergy;
    Strength = strength;
    Defense = defense;
    Agility = agility;
    CreatedAtUtc = createdAtUtc;
  }

  public Guid? Id { get; }
  public string? Name { get; }
  public Faction? Faction { get; }
  public int? Level { get; }
  public long? Experience { get; }
  public long? Gold { get; }
  public int? Energy { get; }
  public int? MaxEnergy { get; }
  public int? Strength { get; }
  public int? Defense { get; }
  public int? Agility { get; }
  public DateTimeOffset? CreatedAtUtc { get; }

  public bool Found => Id is not null;

  public static GetCurrentCharacterResult NotFound()
  {
    return new GetCurrentCharacterResult(
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null
    );
  }

  public static GetCurrentCharacterResult Success(Character character)
  {
    return new GetCurrentCharacterResult(
      character.Id,
      character.Name,
      character.Faction,
      character.Level,
      character.Experience,
      character.Gold,
      character.Energy,
      character.MaxEnergy,
      character.Strength,
      character.Defense,
      character.Agility,
      character.CreatedAtUtc
    );
  }
}
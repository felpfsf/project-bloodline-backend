using Bloodline.Application.Common.Validation;
using Bloodline.Domain.Characters;

namespace Bloodline.Application.Characters.CreateCharacter;

public sealed record CreateCharacterResult
{
  private CreateCharacterResult(
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
    DateTimeOffset? createdAtUtc,
    CreateCharacterError? error,
    IReadOnlyCollection<ValidationError> validationErrors
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
    Error = error;
    ValidationErrors = validationErrors ?? Array.Empty<ValidationError>();
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
  public CreateCharacterError? Error { get; }
  public IReadOnlyCollection<ValidationError> ValidationErrors { get; }

  public static CreateCharacterResult Success(
    Guid id,
    string name,
    Faction faction,
    int level,
    long experience,
    long gold,
    int energy,
    int maxEnergy,
    int strength,
    int defense,
    int agility,
    DateTimeOffset createdAtUtc
  )
  {
    return new CreateCharacterResult(
      id: id,
      name: name,
      faction: faction,
      level: level,
      experience: experience,
      gold: gold,
      energy: energy,
      maxEnergy: maxEnergy,
      strength: strength,
      defense: defense,
      agility: agility,
      createdAtUtc: createdAtUtc,
      error: null,
      validationErrors: []
    );
  }

  public static CreateCharacterResult Failure(
    CreateCharacterError error
  )
  {
    return new CreateCharacterResult(
      id: null,
      name: null,
      faction: null,
      level: null,
      experience: null,
      gold: null,
      energy: null,
      maxEnergy: null,
      strength: null,
      defense: null,
      agility: null,
      createdAtUtc: null,
      error: error,
      validationErrors: []
    );
  }

  public static CreateCharacterResult ValidationFailure(
    IReadOnlyCollection<ValidationError> validationErrors
  )
  {
    return new CreateCharacterResult(
      id: null,
      name: null,
      faction: null,
      level: null,
      experience: null,
      gold: null,
      energy: null,
      maxEnergy: null,
      strength: null,
      defense: null,
      agility: null,
      createdAtUtc: null,
      error: CreateCharacterError.ValidationFailed,
      validationErrors: validationErrors
    );
  }
}
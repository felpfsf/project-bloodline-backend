namespace Bloodline.Domain.Characters;

public sealed class Character
{
  public Guid Id { get; private set; }
  public Guid UserId { get; private set; }
  public string Name { get; private set; } = null!;
  public string NormalizedName { get; private set; } = null!;
  public Faction Faction { get; private set; }
  public int Level { get; private set; }
  public long Experience { get; private set; }
  public long Gold { get; private set; }
  public int Energy { get; private set; }
  public int MaxEnergy { get; private set; }
  public int Strength { get; private set; }
  public int Defense { get; private set; }
  public int Agility { get; private set; }

  public DateTimeOffset CreatedAtUtc { get; private set; }

  private Character() { }

  private Character(
    Guid id,
    Guid userId,
    string name,
    string normalizedName,
    Faction faction,
    int strength,
    int defense,
    int agility,
    DateTimeOffset createdAtUtc
  )
  {
    Id = id;
    UserId = userId;
    Name = name;
    NormalizedName = normalizedName;
    Faction = faction;

    Level = 1;
    Experience = 0;
    Gold = 0;

    Energy = 100;
    MaxEnergy = 100;

    Strength = strength;
    Defense = defense;
    Agility = agility;

    CreatedAtUtc = createdAtUtc;
  }

  public static Character Create(
    Guid userId,
    string name,
    Faction faction,
    DateTimeOffset createdAtUtc
  )
  {
    if (userId == Guid.Empty)
    {
      throw new ArgumentException("User ID is required.", nameof(userId));
    }

    if (string.IsNullOrWhiteSpace(name))
    {
      throw new ArgumentException("Name is required.", nameof(name));
    }

    var normalizedName = NormalizeName(name);

    var stats = GetInitialStats(faction);

    return new Character(
      Guid.NewGuid(),
      userId,
      name,
      normalizedName,
      faction,
      stats.Strength,
      stats.Defense,
      stats.Agility,
      createdAtUtc
    );
  }

  public bool HasEnoughEnergy(int amount)
  {
    if (amount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(amount), "Energy amount must be greater than zero.");
    }
    return Energy >= amount;
  }
  public void ConsumeEnergy(int amount)
  {
    if (amount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(amount), "Energy amount must be greater than zero.");
    }
    if (Energy < amount)
    {
      throw new InvalidOperationException("Character does not have enough energy");
    }
    Energy -= amount;
  }

  public void ReceivedExperience(long experience)
  {
    if (experience < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(experience),
        "Experience amount must be greater than or equal to zero."
      );
    }

    Experience += experience;

    while (Experience >= CharacterProgression.ExperienceRequiredForNextLevel(Level + 1))
    {
      Level++;
    }
  }

  public void ReceiveHuntRewards(long experience, long gold)
  {
    if (experience < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(experience), "Experience reward cannot be negative.");
    }
    if (gold < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(gold), "Gold reward cannot be negative.");
    }

    Experience += experience;
    Gold += gold;
  }

  private static string NormalizeName(string name)
  {
    return name.Trim().ToUpperInvariant();
  }

  private static InitialStats GetInitialStats(Faction faction)
  {
    return faction switch
    {
      Faction.Vampire => new InitialStats(10, 9, 12),
      Faction.Werewolf => new InitialStats(12, 11, 8),
      _ => throw new ArgumentOutOfRangeException(nameof(faction), "Invalid faction")
    };
  }

  private sealed record InitialStats(
    int Strength,
    int Defense,
    int Agility
  );
}
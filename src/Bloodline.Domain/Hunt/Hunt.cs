namespace Bloodline.Domain.Hunt;

public sealed class Hunt
{
  private const string ActiveHuntRequiredMessage = "Only active hunts can be completed.";
  private const string HuntNotFinishedMessage = "Cannot complete hunt before it ends.";

  public Guid Id { get; private set; }
  public Guid CharacterId { get; private set; }

  public HuntStatus Status { get; private set; }

  public DateTimeOffset StartsAtUtc { get; private set; }
  public DateTimeOffset EndsAtUtc { get; private set; }
  public DateTimeOffset? CompletedAtUtc { get; private set; }

  public int EnergyCost { get; private set; }
  public long ExperienceReward { get; private set; }
  public long GoldReward { get; private set; }

  private Hunt() { }

  private Hunt(
    Guid id,
    Guid characterId,
    DateTimeOffset startsAtUtc,
    DateTimeOffset endsAtUtc,
    int energyCost,
    long experienceReward,
    long goldReward
  )
  {
    Id = id;
    CharacterId = characterId;
    Status = HuntStatus.Active;
    StartsAtUtc = startsAtUtc;
    EndsAtUtc = endsAtUtc;
    EnergyCost = energyCost;
    ExperienceReward = experienceReward;
    GoldReward = goldReward;
  }


  public static Hunt Start(
    Guid characterId,
    DateTimeOffset startsAtUtc,
    TimeSpan duration,
    int energyCost,
    long experienceReward,
    long goldReward
  )
  {
    ValidateStartParameters(characterId, duration, energyCost, experienceReward, goldReward);

    return new Hunt(
      Guid.NewGuid(),
      characterId,
      startsAtUtc,
      startsAtUtc.Add(duration),
      energyCost,
      experienceReward,
      goldReward
    );
  }

  public bool CanCompleteAt(DateTimeOffset nowUtc)
  {
    return Status == HuntStatus.Active && nowUtc >= EndsAtUtc;
  }

  public void Complete(DateTimeOffset completedAtUtc)
  {
    EnsureIsActive();
    EnsureHasEndedBy(completedAtUtc);

    Status = HuntStatus.Completed;
    CompletedAtUtc = completedAtUtc;
  }

  private static void ValidateStartParameters(
    Guid characterId,
    TimeSpan duration,
    int energyCost,
    long experienceReward,
    long goldReward
  )
  {
    if (characterId == Guid.Empty)
    {
      throw new ArgumentException("Character ID is required.", nameof(characterId));
    }

    if (duration <= TimeSpan.Zero)
    {
      throw new ArgumentOutOfRangeException(nameof(duration), "Hunt duration must be greater than zero.");
    }

    if (energyCost <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(energyCost), "Energy cost must be greater than zero.");
    }

    if (experienceReward <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(experienceReward), "Experience reward must be greater than zero.");
    }

    if (goldReward <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(goldReward), "Gold reward must be greater than zero.");
    }
  }

  private void EnsureIsActive()
  {
    if (Status != HuntStatus.Active)
    {
      throw new InvalidOperationException(ActiveHuntRequiredMessage);
    }
  }

  private void EnsureHasEndedBy(DateTimeOffset completedAtUtc)
  {
    if (completedAtUtc < EndsAtUtc)
    {
      throw new InvalidOperationException(HuntNotFinishedMessage);
    }
  }
}
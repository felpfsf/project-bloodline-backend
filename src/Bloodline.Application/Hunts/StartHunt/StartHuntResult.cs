using Bloodline.Application.Common.Validation;

namespace Bloodline.Application.Hunts.StartHunt;

public sealed record StartHuntResult
{
  private StartHuntResult(
    Guid? huntId,
    string? definitionCode,
    DateTimeOffset? startsAtUtc,
    DateTimeOffset? endsAtUtc,
    int? energyCost,
    long? experienceReward,
    long? goldReward,
    int? remainingEnergy,
    StartHuntError? error,
    IReadOnlyCollection<ValidationError> validationErrors
  )
  {

    HuntId = huntId;
    DefinitionCode = definitionCode;
    StartsAtUtc = startsAtUtc;
    EndsAtUtc = endsAtUtc;
    EnergyCost = energyCost;
    ExperienceReward = experienceReward;
    GoldReward = goldReward;
    RemainingEnergy = remainingEnergy;
    Error = error;
    ValidationErrors = validationErrors ?? Array.Empty<ValidationError>();
  }

  public Guid? HuntId { get; init; }
  public string? DefinitionCode { get; init; }
  public DateTimeOffset? StartsAtUtc { get; init; }
  public DateTimeOffset? EndsAtUtc { get; init; }
  public int? EnergyCost { get; init; }
  public long? ExperienceReward { get; init; }
  public long? GoldReward { get; init; }
  public int? RemainingEnergy { get; init; }

  public StartHuntError? Error { get; init; }
  public IReadOnlyCollection<ValidationError> ValidationErrors { get; }

  public bool IsSuccess => Error is null;

  public static StartHuntResult Success(
    Guid huntId,
    string definitionCode,
    DateTimeOffset startsAtUtc,
    DateTimeOffset endsAtUtc,
    int energyCost,
    long experienceReward,
    long goldReward,
    int remainingEnergy
  )
  {
    return new StartHuntResult(
      huntId,
      definitionCode,
      startsAtUtc,
      endsAtUtc,
      energyCost,
      experienceReward,
      goldReward,
      remainingEnergy,
      null,
      []
    );
  }

  public static StartHuntResult Failure(StartHuntError error)
  {
    return new StartHuntResult(
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      null,
      error,
      []
    );
  }

}
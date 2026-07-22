using Bloodline.Application.Common.Validation;

namespace Bloodline.Application.Hunts.GetActiveHunt;

public sealed record GetActiveHuntResult
{
  private GetActiveHuntResult(
    Guid? huntId,
    string? definitionCode,
    DateTimeOffset? startsAtUtc,
    DateTimeOffset? endsAtUtc,
    int? energyCost,
    long? experienceReward,
    long? goldReward,
    GetActiveHuntError? error,
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
    Error = error;
    ValidationErrors = validationErrors ?? [];
  }

  public Guid? HuntId { get; }
  public string? DefinitionCode { get; }
  public DateTimeOffset? StartsAtUtc { get; }
  public DateTimeOffset? EndsAtUtc { get; }
  public int? EnergyCost { get; }
  public long? ExperienceReward { get; }
  public long? GoldReward { get; }

  public GetActiveHuntError? Error { get; }
  public IReadOnlyCollection<ValidationError> ValidationErrors { get; }

  public bool IsSuccess => Error is null;
  public bool HasActiveHunt => HuntId is not null;

  public static GetActiveHuntResult Success(
    Guid huntId,
    string definitionCode,
    DateTimeOffset startsAtUtc,
    DateTimeOffset endsAtUtc,
    int energyCost,
    long experienceReward,
    long goldReward
  )
  {
    return new GetActiveHuntResult(
      huntId,
      definitionCode,
      startsAtUtc,
      endsAtUtc,
      energyCost,
      experienceReward,
      goldReward,
      error: null,
      validationErrors: []
    );
  }

  public static GetActiveHuntResult NotActiveHunt()
  {
    return new GetActiveHuntResult(
      huntId: null,
      definitionCode: null,
      startsAtUtc: null,
      endsAtUtc: null,
      energyCost: null,
      experienceReward: null,
      goldReward: null,
      error: null,
      validationErrors: []
    );
  }

  public static GetActiveHuntResult Failure(GetActiveHuntError error)
  {
    return new GetActiveHuntResult(
      huntId: null,
      definitionCode: null,
      startsAtUtc: null,
      endsAtUtc: null,
      energyCost: null,
      experienceReward: null,
      goldReward: null,
      error,
      validationErrors: []
    );
  }

}
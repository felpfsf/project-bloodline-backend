using Bloodline.Application.Common.Validation;

namespace Bloodline.Application.Hunts.GetActiveHunt;

public sealed record GetActiveHuntResult
{
  private GetActiveHuntResult(
    Guid? huntId,
    string? definitionCode,
    DateTimeOffset? startedAtUtc,
    DateTimeOffset? endedAtUtc,
    int? energyCost,
    long? experienceReward,
    long? goldReward,
    GetActiveHuntError? error,
    IReadOnlyCollection<ValidationError> validationErrors
  )
  {
    HuntId = huntId;
    DefinitionCode = definitionCode;
    StartedAtUtc = startedAtUtc;
    EndedAtUtc = endedAtUtc;
    EnergyCost = energyCost;
    ExperienceReward = experienceReward;
    GoldReward = goldReward;
    Error = error;
    ValidationErrors = validationErrors ?? [];
  }

  public Guid? HuntId { get; }
  public string? DefinitionCode { get; }
  public DateTimeOffset? StartedAtUtc { get; }
  public DateTimeOffset? EndedAtUtc { get; }
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
    DateTimeOffset startedAtUtc,
    DateTimeOffset endedAtUtc,
    int energyCost,
    long experienceReward,
    long goldReward
  )
  {
    return new GetActiveHuntResult(
      huntId,
      definitionCode,
      startedAtUtc,
      endedAtUtc,
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
      startedAtUtc: null,
      endedAtUtc: null,
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
      startedAtUtc: null,
      endedAtUtc: null,
      energyCost: null,
      experienceReward: null,
      goldReward: null,
      error,
      validationErrors: []
    );
  }

}
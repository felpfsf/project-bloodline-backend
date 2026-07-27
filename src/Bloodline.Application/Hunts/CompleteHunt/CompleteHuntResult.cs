namespace Bloodline.Application.Hunts.CompleteHunt;

public sealed record CompleteHuntResult
{
  private CompleteHuntResult(
    Guid? huntId,
    string? definitionCode,
    DateTimeOffset? completedAtUtc,
    long? experienceReward,
    long? goldReward,
    CompleteHuntError? error
  )
  {
    HuntId = huntId;
    DefinitionCode = definitionCode;
    CompletedAtUtc = completedAtUtc;
    ExperienceReward = experienceReward;
    GoldReward = goldReward;
    Error = error;
  }

  public Guid? HuntId { get; }
  public string? DefinitionCode { get; }
  public DateTimeOffset? CompletedAtUtc { get; }
  public long? ExperienceReward { get; }
  public long? GoldReward { get; }

  public CompleteHuntError? Error { get; }

  public bool IsSuccess => Error is null;

  public static CompleteHuntResult Success(
    Guid huntId,
    string definitionCode,
    DateTimeOffset completedAtUtc,
    long experienceReward,
    long goldReward
  ) => new(
    huntId: huntId,
    definitionCode: definitionCode,
    completedAtUtc: completedAtUtc,
    experienceReward: experienceReward,
    goldReward: goldReward,
    error: null
  );

  public static CompleteHuntResult Failure(CompleteHuntError error) => new(
    huntId: null,
    definitionCode: null,
    completedAtUtc: null,
    experienceReward: null,
    goldReward: null,
    error: error
  );

}
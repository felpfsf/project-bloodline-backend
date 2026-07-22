namespace Bloodline.Application.Hunts.CompleteHunt;

public sealed record CompleteHuntResult
{
  private CompleteHuntResult(
    Guid? huntId,
    string? definitionCode,
    DateTimeOffset? startsAtUtc,
    long? experienceReward,
    long? goldReward,
    CompleteHuntError? error
  )
  {
    HuntId = huntId;
    DefinitionCode = definitionCode;
    StartsAtUtc = startsAtUtc;
    ExperienceReward = experienceReward;
    GoldReward = goldReward;
    Error = error;
  }

  public Guid? HuntId { get; }
  public string? DefinitionCode { get; }
  public DateTimeOffset? StartsAtUtc { get; }
  public long? ExperienceReward { get; }
  public long? GoldReward { get; }

  public CompleteHuntError? Error { get; }

  public bool IsSuccess => Error is null;

  public static CompleteHuntResult Success(
    Guid huntId,
    string definitionCode,
    DateTimeOffset startsAtUtc,
    long experienceReward,
    long goldReward
  ) => new(
    huntId: huntId,
    definitionCode: definitionCode,
    startsAtUtc: startsAtUtc,
    experienceReward: experienceReward,
    goldReward: goldReward,
    error: null
  );

  public static CompleteHuntResult Failure(CompleteHuntError error) => new(
    huntId: null,
    definitionCode: null,
    startsAtUtc: null,
    experienceReward: null,
    goldReward: null,
    error: error
  );

}
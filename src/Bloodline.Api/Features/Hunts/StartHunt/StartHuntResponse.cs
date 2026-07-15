namespace Bloodline.Api.Features.Hunts.StartHunt;

public sealed record StartHuntResponse(
  Guid HuntId,
  string DefinitionCode,
  DateTimeOffset StartsAtUtc,
  DateTimeOffset EndsAtUtc,
  int EnergyCost,
  long ExperienceReward,
  long GoldReward,
  int RemainingAttempts
);
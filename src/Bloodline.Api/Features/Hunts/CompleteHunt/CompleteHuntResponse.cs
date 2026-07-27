namespace Bloodline.Api.Features.Hunts.CompleteHunt;

public sealed record CompleteHuntResponse(
  Guid HuntId,
  string DefinitionCode,
  DateTimeOffset CompletedAtUtc,
  long ExperienceReward,
  long GoldReward
);
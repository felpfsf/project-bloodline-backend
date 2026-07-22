namespace Bloodline.Api.Features.Hunts.GetActiveHunt;

public sealed record GetActiveHuntResponse(
  ActiveHuntResponse? ActiveHunt
);

public sealed record ActiveHuntResponse(
  Guid Id,
  string DefinitionCode,
  DateTimeOffset StartsAtUtc,
  DateTimeOffset EndsAtUtc,
  int EnergyCost,
  long ExperienceReward,
  long GoldReward
);
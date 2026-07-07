namespace Bloodline.Application.Hunts;

public sealed record HuntDefinition(
  string Code,
  TimeSpan Duration,
  int EnergyCost,
  long ExperienceReward,
  long GoldReward
);
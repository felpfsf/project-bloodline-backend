namespace Bloodline.Application.Hunts;

public static class HuntDefinitions
{
  public static readonly HuntDefinition ShadowHunt = new(
    Code: "shadow_hunt",
    Duration: TimeSpan.FromSeconds(60),
    EnergyCost: 10,
    ExperienceReward: 25,
    GoldReward: 15
  );
}
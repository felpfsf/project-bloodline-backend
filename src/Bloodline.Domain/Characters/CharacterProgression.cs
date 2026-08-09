namespace Bloodline.Domain.Characters;

public static class CharacterProgression
{
  public static long ExperienceRequiredForNextLevel(int currentLevel)
  {
    if (currentLevel < 1)
    {
      throw new ArgumentOutOfRangeException(
        nameof(currentLevel),
        "Level must be greater than or equal to 1."
      );
    }

    return 50L * currentLevel * (currentLevel - 1);
  }
}
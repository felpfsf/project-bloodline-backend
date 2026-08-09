using Bloodline.Domain.Characters;

namespace Bloodline.Domain.Tests.Characters;

public sealed class CharacterProgressionTests
{
  [Theory]
  [InlineData(1, 0)]
  [InlineData(2, 100)]
  [InlineData(3, 300)]
  [InlineData(4, 600)]
  [InlineData(5, 1000)]
  [InlineData(10, 4500)]
  public void Experience_Required_For_Next_Level_Should_Return_Correct_Threshold(int currentLevel, long expectedExperience)
  {
    // Act
    var result = CharacterProgression.ExperienceRequiredForNextLevel(currentLevel);

    // Assert
    Assert.Equal(expectedExperience, result);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-1)]
  public void Experience_Required_For_Next_Level_Should_Throw_When_Current_Level_Is_Less_Than_1(int currentLevel)
  {
    // Act & Assert
    Assert.Throws<ArgumentOutOfRangeException>(
      () => CharacterProgression.ExperienceRequiredForNextLevel(currentLevel)
    );
  }
}
using Bloodline.Domain.Characters;

namespace Bloodline.Domain.Tests.Characters;

public sealed class CharacterTests
{

  [Fact]
  public void ConsumeEnergy_Should_Decrease_Energy_When_Character_Has_Enough_Energy()
  {
    // Arrange
    var character = CreateCharacter();

    // Act
    character.ConsumeEnergy(50);

    // Assert
    Assert.Equal(50, character.Energy);
  }

  [Fact]
  public void ConsumeEnergy_Should_Throw_When_Character_Does_Not_Have_Enough_Energy()
  {
    // Arrange
    var character = CreateCharacter();

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => character.ConsumeEnergy(150));
  }

  [Fact]
  public void ConsumeEnergy_Should_Throw_When_Amount_Is_Zero_or_Negative()
  {
    // Arrange
    var character = CreateCharacter();

    // Act & Assert
    Assert.Throws<ArgumentOutOfRangeException>(() => character.ConsumeEnergy(0));
    Assert.Throws<ArgumentOutOfRangeException>(() => character.ConsumeEnergy(-10));
  }

  [Fact]
  public void HasEnoughEnergy_Should_Return_True_When_Character_Has_Enough_Energy()
  {
    // Arrange
    var character = CreateCharacter();

    // Act
    var result = character.HasEnoughEnergy(50);

    // Assert
    Assert.True(result);
  }

  [Fact]
  public void HasEnoughEnergy_Should_Return_False_When_Character_Does_Not_Have_Enough_Energy()
  {
    // Arrange
    var character = CreateCharacter();

    // Act
    var result = character.HasEnoughEnergy(150);

    // Assert
    Assert.False(result);
  }

  [Fact]
  public void ReceiveHuntReward_Should_Increase_Character_Experience_And_Gold()
  {
    // Arrange
    var character = CreateCharacter();

    // Act
    character.ReceiveHuntRewards(100, 50);

    // Assert
    Assert.Equal(100, character.Experience);
    Assert.Equal(50, character.Gold);
  }

  [Fact]
  public void ReceiveHuntReward_Should_Allow_Only_Experience()
  {
    // Arrange
    var character = CreateCharacter();

    // Act
    character.ReceiveHuntRewards(100, 0);

    // Assert
    Assert.Equal(100, character.Experience);
    Assert.Equal(0, character.Gold);
  }

  [Fact]
  public void ReceiveHuntReward_Should_Throw_When_Gold_Is_Negative()
  {
    // Arrange
    var character = CreateCharacter();

    // Act & Assert
    Assert.Throws<ArgumentOutOfRangeException>(() => character.ReceiveHuntRewards(100, -50));
  }

  private static Character CreateCharacter()
  {
    return Character.Create(
      Guid.NewGuid(),
      "Damon Salvatore",
      Faction.Vampire,
      DateTimeOffset.UtcNow
    );
  }
}
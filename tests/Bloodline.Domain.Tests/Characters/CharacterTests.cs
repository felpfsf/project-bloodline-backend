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

  [Theory]
  [InlineData(0)]
  [InlineData(-1)]
  public void HasEnoughEnergy_ShouldThrow_WhenAmountIsNotPositive(int amount)
  {
    var character = CreateCharacter();
    Assert.Throws<ArgumentOutOfRangeException>(() =>
      character.HasEnoughEnergy(amount)
    );
  }

  [Fact]
  public void Received_Experience_Should_Increase_Character_Experience()
  {
    // Arrange
    var character = CreateCharacter();

    // Act
    character.ReceivedExperience(50);

    // Assert
    Assert.Equal(50, character.Experience);
  }

  [Fact]
  public void Received_Experience_Should_Level_Up_When_Experience_Reaches_Threshold()
  {
    // Arrange
    var character = CreateCharacter();

    // Act
    character.ReceivedExperience(100);

    // Assert
    Assert.Equal(100, character.Experience);
    Assert.Equal(2, character.Level);
  }

  [Fact]
  public void Received_Experience_Should_Not_Level_Up_When_Experience_Is_Below_Threshold()
  {
    // Arrange
    var character = CreateCharacter();

    // Act
    character.ReceivedExperience(50);

    // Assert
    Assert.Equal(50, character.Experience);
    Assert.Equal(1, character.Level);
  }

  [Fact]
  public void Received_Experience_Should_Allow_Multiple_Level_Ups()
  {
    // Arrange
    var character = CreateCharacter();

    // Act
    character.ReceivedExperience(600);

    // Assert
    Assert.Equal(600, character.Experience);
    Assert.Equal(4, character.Level);
  }

  [Fact]
  public void Received_Experience_Should_Accumulate_Experience_Across_Multiple_Rewards()
  {
    // Arrange
    var character = CreateCharacter();

    // Act
    character.ReceivedExperience(50);
    character.ReceivedExperience(30);
    character.ReceivedExperience(20);

    // Assert
    Assert.Equal(100, character.Experience);
    Assert.Equal(2, character.Level);
  }

  [Fact]
  public void Received_Experience_Should_Throw_When_Experience_Is_Negative()
  {
    // Arrange
    var character = CreateCharacter();

    // Act & Assert
    Assert.Throws<ArgumentOutOfRangeException>(() => character.ReceivedExperience(-100));
  }

  [Theory]
  [MemberData(nameof(ReceivedExperienceCases))]
  public void Received_Experience_Should_Update_Experience_And_Level_Correctly(long gainedExperience, long expectedExperience, int expectedLevel)
  {
    // Arrange
    var character = CreateCharacter();

    // Act
    character.ReceivedExperience(gainedExperience);

    // Assert
    Assert.Equal(expectedExperience, character.Experience);
    Assert.Equal(expectedLevel, character.Level);
  }

  [Fact]
  public void ReceivedExperience_Should_Accumulate_Across_Multiple_Calls()
  {
    // Arrange
    var character = CreateCharacter();

    // Act
    character.ReceivedExperience(50);
    character.ReceivedExperience(30);
    character.ReceivedExperience(20);

    // Assert
    Assert.Equal(100, character.Experience);
    Assert.Equal(2, character.Level);
  }



  public static TheoryData<long, long, int> ReceivedExperienceCases => new()
  {
    {50L, 50L, 1}, // bellow lvl 2
    {99L, 99L, 1}, // lower border
    {100L, 100L, 2}, // lvl 2
    {299L, 299L, 2}, // bellow lvl 3
    {300L, 300L, 3}, // lvl 3
    {600L, 600L, 4} // multiple level ups
  };

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
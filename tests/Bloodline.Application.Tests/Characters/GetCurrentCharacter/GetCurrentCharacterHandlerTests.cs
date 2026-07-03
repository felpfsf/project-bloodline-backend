using Bloodline.Application.Characters.GetCurrentCharacter;
using Bloodline.Application.Tests.Characters.Fakes;
using Bloodline.Domain.Characters;
using FluentAssertions;

namespace Bloodline.Application.Tests.Characters.GetCurrentCharacter;

public sealed class GetCurrentCharacterHandlerTests
{




  [Fact]
  public async Task HandleAsync_Should_Return_NotFound_When_User_Has_No_Character()
  {
    // Arrange
    var repository = new FakeCharacterRepository();
    var handler = new GetCurrentCharacterHandler(repository);
    var userId = Guid.NewGuid();

    // Act
    var result = await handler.HandleAsync(
        new GetCurrentCharacterQuery(userId),
        CancellationToken.None
      );

    // Assert
    result.Found.Should().BeFalse();
    result.Id.Should().BeNull();
    repository.RequestedUserId.Should().Be(userId);
  }

  [Fact]
  public async Task HandleAsync_Should_Return_Character_When_User_Has_Character()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var character = Character.Create(
      userId,
      "Damon",
      Faction.Vampire,
      new DateTimeOffset(2026, 7, 3, 12, 0, 0, TimeSpan.Zero)
    );

    var repository = new FakeCharacterRepository { CharacterToReturn = character };
    var handler = new GetCurrentCharacterHandler(repository);

    // Act
    var result = await handler.HandleAsync(
        new GetCurrentCharacterQuery(userId),
        CancellationToken.None
      );

    // Assert
    result.Found.Should().BeTrue();
    result.Id.Should().Be(character.Id);
    result.Name.Should().Be(character.Name);
    result.Faction.Should().Be(character.Faction);
    result.Level.Should().Be(1);
    result.Energy.Should().Be(100);
    result.CreatedAtUtc.Should().Be(character.CreatedAtUtc);

    repository.RequestedUserId.Should().Be(userId);
  }
}
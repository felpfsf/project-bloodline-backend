using FluentAssertions;
using DomainCharacter = Bloodline.Domain.Character.Character;
using Faction = Bloodline.Domain.Character.Faction;

namespace Bloodline.Domain.Tests.Character;

public sealed class CharacterTests
{
  [Fact]
  public void Create_Should_Create_Vampire_With_Expected_Initial_State()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var createdAtUtc = new DateTime(2026, 6, 29, 18, 0, 0, DateTimeKind.Utc);

    // Act
    var character = DomainCharacter.Create(
        userId,
        "Draven",
        Faction.Vampire,
        createdAtUtc);

    // Assert
    character.UserId.Should().Be(userId);
    character.Name.Should().Be("Draven");
    character.NormalizedName.Should().Be("DRAVEN");
    character.Faction.Should().Be(Faction.Vampire);

    character.Level.Should().Be(1);
    character.Experience.Should().Be(0);
    character.Gold.Should().Be(0);

    character.Energy.Should().Be(100);
    character.MaxEnergy.Should().Be(100);

    character.Strength.Should().Be(10);
    character.Defense.Should().Be(9);
    character.Agility.Should().Be(12);

    character.CreatedAtUtc.Should().Be(createdAtUtc);
  }
}

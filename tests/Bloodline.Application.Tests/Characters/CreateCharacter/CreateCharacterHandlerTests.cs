

using Bloodline.Application.Characters.CreateCharacter;
using Bloodline.Application.Tests.Characters.Fakes;
using Bloodline.Application.Tests.Fakes;
using Bloodline.Domain.Characters;
using FluentAssertions;

namespace Bloodline.Application.Tests.Characters.CreateCharacter;

public sealed class CreateCharacterHandlerTests
{
  private readonly FakeCharacterRepository _characterRepository = new();
  private readonly FakeClock _clock = new()
  {
    UtcNow = new DateTimeOffset(2026, 6, 30, 12, 0, 0, TimeSpan.Zero)
  };

  private readonly CreateCharacterCommandValidator _validator = new();

  private CreateCharacterHandler CreateSut()
  {
    return new CreateCharacterHandler(
      _characterRepository,
      _clock,
      _validator
    );
  }

  private static CreateCharacterCommand ValidCommand(
    Guid? userId = null,
    string? name = "Draven",
    Faction faction = Faction.Vampire
  )
  {
    return new CreateCharacterCommand(
      UserId: userId ?? Guid.NewGuid(),
      Name: name ?? "Draven",
      Faction: faction
    );
  }

  [Fact]
  public async Task HandleAsync_Should_Create_Vampire_When_Command_Is_Valid()
  {
    // Arrange
    var command = ValidCommand();
    var handler = CreateSut();

    // Act
    var result = await handler.HandleAsync(command, CancellationToken.None);

    // Assert
    result.Error.Should().BeNull();
    result.ValidationErrors.Should().BeEmpty();
    result.Id.Should().NotBeNull();
    result.Name.Should().Be("Draven");
    result.Faction.Should().Be(Faction.Vampire);
    result.Level.Should().Be(1);
    result.Experience.Should().Be(0);
    result.Gold.Should().Be(0);
    result.Energy.Should().Be(100);
    result.MaxEnergy.Should().Be(100);
    result.Strength.Should().Be(10);
    result.Defense.Should().Be(9);
    result.Agility.Should().Be(12);
    result.CreatedAtUtc.Should().Be(_clock.UtcNow);
    _characterRepository.CheckedUserId.Should().Be(command.UserId);
    _characterRepository.CheckedNormalizedName.Should().Be("DRAVEN");
    _characterRepository.CreatedCharacter.Should().NotBeNull();
    _characterRepository.CreatedCharacter!.UserId.Should().Be(command.UserId);
    _characterRepository.CreatedCharacter.Name.Should().Be("Draven");
    _characterRepository.CreatedCharacter.NormalizedName.Should().Be("DRAVEN");
  }

  [Fact]
  public async Task HandleAsync_Should_Create_Werewolf_When_Command_Is_Valid()
  {
    // Arrange
    var command = ValidCommand(name: "Fenrir", faction: Faction.Werewolf);
    var handler = CreateSut();

    // Act
    var result = await handler.HandleAsync(command, CancellationToken.None);

    // Assert
    result.Error.Should().BeNull();
    result.ValidationErrors.Should().BeEmpty();
    result.Id.Should().NotBeNull();
    result.Name.Should().Be("Fenrir");
    result.Faction.Should().Be(Faction.Werewolf);
    result.Level.Should().Be(1);
    result.Experience.Should().Be(0);
    result.Gold.Should().Be(0);
    result.Energy.Should().Be(100);
    result.MaxEnergy.Should().Be(100);
    result.Strength.Should().Be(12);
    result.Defense.Should().Be(11);
    result.Agility.Should().Be(8);
    result.CreatedAtUtc.Should().Be(_clock.UtcNow);
  }

  [Fact]
  public async Task HandleAsync_Should_Return_ValidationFailure_And_Not_Use_Repository_When_Name_Is_Invalid()
  {
    // Arrange
    var command = ValidCommand(name: "F");
    var handler = CreateSut();

    // Act
    var result = await handler.HandleAsync(command, CancellationToken.None);

    // Assert
    result.Error.Should().Be(CreateCharacterError.ValidationFailed);
    result.ValidationErrors.Should()
      .ContainSingle(error =>
        error.PropertyName == "Name" &&
        error.ErrorCode == CreateCharacterErrorCodes.NameMinLength
    );
    _characterRepository.CheckedUserId.Should().BeNull();
    _characterRepository.CheckedNormalizedName.Should().BeNull();
    _characterRepository.CreatedCharacter.Should().BeNull();
  }

  [Fact]
  public async Task HandleAsync_Should_Return_NameAlreadyTaken_When_Name_Is_Already_Used()
  {
    // Arrange
    _characterRepository.NameAlreadyTaken = true;

    var command = ValidCommand(name: "Draven");
    var handler = CreateSut();

    // Act
    var result = await handler.HandleAsync(command, CancellationToken.None);

    // Assert
    result.Error.Should().Be(CreateCharacterError.NameAlreadyTaken);
    _characterRepository.CheckedUserId.Should().Be(command.UserId);
    _characterRepository.CheckedNormalizedName.Should().Be("DRAVEN");
    _characterRepository.CreatedCharacter.Should().BeNull();
  }

  [Fact]
  public async Task HandleAsync_Should_Return_UserAlreadyHasCharacter_When_UserId_UniqueConstraint_Is_Violated()
  {
    // Arrange
    _characterRepository.UniqueConstraintToThrow = "ux_characters_user_id";

    var commannd = ValidCommand();
    var handler = CreateSut();

    // Act
    var result = await handler.HandleAsync(commannd, CancellationToken.None);

    // Assert
    result.Error.Should().Be(CreateCharacterError.UserAlreadyHasCharacter);
  }

  [Fact]
  public async Task HandleAsync_Should_Return_NameAlreadyTaken_When_NormalizedName_UniqueConstraint_Is_Violated()
  {
    // Arrange
    _characterRepository.UniqueConstraintToThrow = "ux_characters_normalized_name";

    var commannd = ValidCommand();
    var handler = CreateSut();

    // Act
    var result = await handler.HandleAsync(commannd, CancellationToken.None);

    // Assert
    result.Error.Should().Be(CreateCharacterError.NameAlreadyTaken);
  }
}
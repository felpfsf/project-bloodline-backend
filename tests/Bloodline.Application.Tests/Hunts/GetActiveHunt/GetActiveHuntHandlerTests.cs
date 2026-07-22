using Bloodline.Application.Hunts.GetActiveHunt;
using Bloodline.Application.Tests.Characters.Fakes;
using Bloodline.Application.Tests.Hunts.Fakes;
using Bloodline.Domain.Characters;
using Bloodline.Domain.Hunts;

namespace Bloodline.Application.Tests.Hunts.GetActiveHunt;

public sealed class GetActiveHuntHandlerTests
{

  [Fact]
  public async Task HandleAsync_Should_Return_CharacterNotFound_When_Character_Does_Not_Exist()
  {
    var handler = CreateHandler();

    var result = await handler.HandleAsync(
      new GetActiveHuntQuery(Guid.NewGuid()),
      CancellationToken.None
    );

    Assert.False(result.IsSuccess);
    Assert.Equal(GetActiveHuntError.CharacterNotFound, result.Error);
  }

  [Fact]
  public async Task HandleAsync_Should_Return_NotActiveHunt_When_Hunt_Does_Not_Exist()
  {
    var userId = Guid.NewGuid();
    var character = CreateCharacter(userId);

    var handler = CreateHandler(character);

    var result = await handler.HandleAsync(
      new GetActiveHuntQuery(userId),
      CancellationToken.None
    );

    Assert.True(result.IsSuccess);
    Assert.False(result.HasActiveHunt);
    Assert.Null(result.HuntId);
  }

  [Fact]
  public async Task HandleAsync_Should_Return_Active_Hunt_When_Character_Has_Active_Hunt()
  {
    var userId = Guid.NewGuid();
    var character = CreateCharacter(userId);

    var startsAtUtc = new DateTimeOffset(2026, 7, 7, 20, 0, 0, TimeSpan.Zero);

    var hunt = Hunt.Start(
      character.Id,
      definitionCode: "shadow_hunt",
      startsAtUtc,
      duration: TimeSpan.FromSeconds(60),
      energyCost: 10,
      experienceReward: 25,
      goldReward: 15
    );

    var handler = CreateHandler(character, hunt);

    var result = await handler.HandleAsync(
      new GetActiveHuntQuery(userId),
      CancellationToken.None
    );

    Assert.True(result.IsSuccess);
    Assert.True(result.HasActiveHunt);
    Assert.Equal(hunt.Id, result.HuntId);
    Assert.Equal("shadow_hunt", result.DefinitionCode);
    Assert.Equal(hunt.StartsAtUtc, result.StartedAtUtc);
    Assert.Equal(startsAtUtc.AddSeconds(60), result.EndedAtUtc);
    Assert.Equal(10, result.EnergyCost);
    Assert.Equal(25, result.ExperienceReward);
    Assert.Equal(15, result.GoldReward);
  }

  private static Character CreateCharacter(Guid userId)
  {
    return Character.Create(
      userId,
      name: "Damon Salvatore",
      faction: Faction.Vampire,
      new DateTimeOffset(2026, 7, 7, 20, 0, 0, TimeSpan.Zero)
    );
  }

  private static GetActiveHuntHandler CreateHandler(
    Character? character = null,
    Hunt? activeHunt = null
  )
  {
    var characterRepository = new FakeCharacterRepository()
    {
      CharacterToReturn = character
    };

    var huntRepository = new FakeHuntRepository()
    {
      ActiveHuntToReturn = activeHunt
    };

    return new GetActiveHuntHandler(characterRepository, huntRepository);
  }
}
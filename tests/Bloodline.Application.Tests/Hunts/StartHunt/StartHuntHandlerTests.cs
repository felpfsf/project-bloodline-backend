using Bloodline.Application.Hunts.StartHunt;
using Bloodline.Application.Tests.Characters.Fakes;
using Bloodline.Application.Tests.Fakes;
using Bloodline.Application.Tests.Hunts.Fakes;
using Bloodline.Domain.Characters;

namespace Bloodline.Application.Tests.Hunts.StartHunt;

public sealed class StartHuntHandlerTests
{
  [Fact]
  public async Task HandleAsync_Should_Start_ShadowHunt_When_Character_Can_Start()
  {
    var userId = Guid.NewGuid();
    var nowUtc = new DateTimeOffset(2024, 7, 7, 20, 0, 0, TimeSpan.Zero);
    var character = CreateCharacter(userId);
    var characterRepository = new FakeCharacterRepository
    {
      CharacterToReturn = character
    };
    var huntRepository = new FakeHuntRepository();
    var unitOfWork = new FakeUnitOfWork();
    var clock = new FakeClock
    {
      UtcNow = nowUtc
    };

    var handler = new StartHuntHandler(
      characterRepository, huntRepository, unitOfWork, clock
    );

    var result = await handler.HandleAsync(new StartHuntCommand(userId), CancellationToken.None);

    Assert.True(result.IsSuccess);

    Assert.Equal(userId, characterRepository.RequestedUserIdForUpdate);

    Assert.NotNull(huntRepository.CreatedHunt);
    Assert.Equal(character.Id, huntRepository.CreatedHunt.CharacterId);

    Assert.Equal("shadow_hunt", huntRepository.CreatedHunt.DefinitionCode);
    Assert.Equal(nowUtc, huntRepository.CreatedHunt.StartsAtUtc);
    Assert.Equal(nowUtc.AddSeconds(60), huntRepository.CreatedHunt.EndsAtUtc);

    Assert.Equal(90, character.Energy);
    Assert.Equal(1, unitOfWork.SaveChangesCallCount);

    Assert.Equal(huntRepository.CreatedHunt.Id, result.HuntId);
    Assert.Equal("shadow_hunt", result.DefinitionCode);
    Assert.Equal(nowUtc, result.StartsAtUtc);
    Assert.Equal(nowUtc.AddSeconds(60), result.EndsAtUtc);

    Assert.Equal(10, result.EnergyCost);
    Assert.Equal(25, result.ExperienceReward);
    Assert.Equal(15, result.GoldReward);
    Assert.Equal(90, result.RemainingEnergy);


  }
  [Fact]
  public async Task HandleAsync_Should_Return_CharacterNotFound_When_Character_Does_Not_Exist()
  {
    var characterRepository = new FakeCharacterRepository
    {
      CharacterToReturn = null
    };
    var huntRepository = new FakeHuntRepository();
    var unitOfWork = new FakeUnitOfWork();
    var clock = new FakeClock();
    var handler = new StartHuntHandler(characterRepository, huntRepository, unitOfWork, clock);

    var result = await handler.HandleAsync(new StartHuntCommand(Guid.NewGuid()), CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Equal(StartHuntError.CharacterNotFound, result.Error);

    Assert.Null(huntRepository.CreatedHunt);
    Assert.Equal(0, unitOfWork.SaveChangesCallCount);
  }
  [Fact]
  public async Task HandleAsync_Should_Return_HuntAlreadyActive_When_Character_Has_Active_Hunt()
  {
    var userId = Guid.NewGuid();
    var characterRepository = new FakeCharacterRepository
    {
      CharacterToReturn = CreateCharacter(userId)
    };
    var huntRepository = new FakeHuntRepository
    {
      HasActiveHunt = true
    };
    var unitOfWork = new FakeUnitOfWork();
    var clock = new FakeClock();
    var handler = new StartHuntHandler(characterRepository, huntRepository, unitOfWork, clock);

    var result = await handler.HandleAsync(new StartHuntCommand(userId), CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Equal(StartHuntError.HuntAlreadyActive, result.Error);

    Assert.Null(huntRepository.CreatedHunt);
    Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    Assert.Equal(100, characterRepository.CharacterToReturn!.Energy);

  }
  [Fact]
  public async Task HandleAsync_Should_Return_NotEnoughEnergy_When_Character_Cannot_Pay_Cost()
  {
    var userId = Guid.NewGuid();
    var character = CreateCharacter(userId);

    character.ConsumeEnergy(95);

    var characterRepository = new FakeCharacterRepository
    {
      CharacterToReturn = character
    };
    var huntRepository = new FakeHuntRepository();
    var unitOfWork = new FakeUnitOfWork();
    var clock = new FakeClock();
    var handler = new StartHuntHandler(characterRepository, huntRepository, unitOfWork, clock);

    var result = await handler.HandleAsync(new StartHuntCommand(userId), CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.Equal(StartHuntError.NotEnoughEnergy, result.Error);

    Assert.Null(huntRepository.CreatedHunt);
    Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    Assert.Equal(5, characterRepository.CharacterToReturn!.Energy);
  }

  private static Character CreateCharacter(Guid userId)
  {
    return Character.Create(
      userId,
      "Damon Salvatore",
      Faction.Vampire,
      new DateTimeOffset(2024, 7, 7, 20, 0, 0, TimeSpan.Zero)
    );
  }
}
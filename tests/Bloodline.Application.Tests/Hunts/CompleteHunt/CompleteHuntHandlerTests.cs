using Bloodline.Application.Abstractions.Events;
using Bloodline.Application.Common.Persistence;
using Bloodline.Application.Hunts.CompleteHunt;
using Bloodline.Application.Tests.Characters.Fakes;
using Bloodline.Application.Tests.Fakes;
using Bloodline.Application.Tests.Hunts.Fakes;
using Bloodline.Domain.Characters;
using Bloodline.Domain.Characters.Events;
using Bloodline.Domain.Common;
using Bloodline.Domain.Hunts;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bloodline.Application.Tests.Hunts.CompleteHunt;

public sealed class CompleteHuntHandlerTests
{
  [Fact]
  public async Task HandleAsync_Should_Complete_Hunt_And_Apply_Reward_When_EndTime_Has_Arrived()
  {
    var userId = Guid.NewGuid();
    var startsAtUtc = new DateTimeOffset(2026, 7, 7, 20, 0, 0, TimeSpan.Zero);
    var character = CreateCharacter(userId);
    var hunt = CreateHunt(character, startsAtUtc);
    var characterRepository = new FakeCharacterRepository
    {
      CharacterToReturn = character
    };
    var huntRepository = new FakeHuntRepository
    {
      HuntToReturn = hunt
    };
    var unitOfWork = new FakeUnitOfWork();
    var clock = new FakeClock
    {
      UtcNow = startsAtUtc.AddSeconds(60)
    };
    var handler = CreateHandler(characterRepository, huntRepository, unitOfWork, clock);
    var result = await CompleteHuntAsync(handler, userId, hunt.Id);
    Assert.True(result.IsSuccess);
    Assert.Equal(HuntStatus.Completed, hunt.Status);
    Assert.Equal(clock.UtcNow, hunt.CompletedAtUtc);
    Assert.Equal(25, character.Experience);
    Assert.Equal(15, character.Gold);
    Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    Assert.Equal(hunt.Id, result.HuntId);
    Assert.Equal("shadow_hunt", result.DefinitionCode);
    Assert.Equal(clock.UtcNow, result.CompletedAtUtc);
    Assert.Equal(25, result.ExperienceReward);
    Assert.Equal(15, result.GoldReward);
  }

  [Fact]
  public async Task HandleAsync_Should_Return_Not_Finished_Yet_When_EndTime_Has_Not_Arrived()
  {
    var userId = Guid.NewGuid();
    var startsAtUtc = new DateTimeOffset(2026, 7, 7, 20, 0, 0, TimeSpan.Zero);
    var character = CreateCharacter(userId);
    var hunt = CreateHunt(character, startsAtUtc);
    var characterRepository = new FakeCharacterRepository
    {
      CharacterToReturn = character
    };
    var huntRepository = new FakeHuntRepository
    {
      HuntToReturn = hunt
    };
    var unitOfWork = new FakeUnitOfWork();
    var clock = new FakeClock
    {
      UtcNow = startsAtUtc.AddSeconds(58)
    };
    var handler = CreateHandler(characterRepository, huntRepository, unitOfWork, clock);
    var result = await CompleteHuntAsync(handler, userId, hunt.Id);
    Assert.False(result.IsSuccess);
    Assert.Equal(CompleteHuntError.HuntNotFinishedYet, result.Error);
    Assert.Equal(HuntStatus.Active, hunt.Status);
    Assert.Equal(0, character.Experience);
    Assert.Equal(0, character.Gold);
    Assert.Equal(0, unitOfWork.SaveChangesCallCount);
  }

  [Fact]
  public async Task HandleAsync_Should_Return_Success_Without_Applying_Reward_When_HuntIs_Already_Completed()
  {
    var userId = Guid.NewGuid();
    var startsAtUtc = new DateTimeOffset(2026, 7, 7, 20, 0, 0, TimeSpan.Zero);
    var character = CreateCharacter(userId);
    var hunt = CreateHunt(character, startsAtUtc);
    hunt.Complete(startsAtUtc.AddSeconds(60));
    var characterRepository = new FakeCharacterRepository
    {
      CharacterToReturn = character
    };
    var huntRepository = new FakeHuntRepository
    {
      HuntToReturn = hunt
    };
    var unitOfWork = new FakeUnitOfWork();
    var clock = new FakeClock
    {
      UtcNow = startsAtUtc.AddSeconds(60)
    };
    var handler = CreateHandler(characterRepository, huntRepository, unitOfWork, clock);
    var result = await CompleteHuntAsync(handler, userId, hunt.Id);
    Assert.True(result.IsSuccess);
    Assert.Equal(HuntStatus.Completed, hunt.Status);
    Assert.Equal(clock.UtcNow, hunt.CompletedAtUtc);
    Assert.Equal(0, character.Experience);
    Assert.Equal(0, character.Gold);
    Assert.Equal(0, unitOfWork.SaveChangesCallCount);
  }

  [Fact]
  public async Task HandleAsync_Should_Dispatch_LevelUp_After_Saving_Without_Duplicating_On_Retry()
  {
    var userId = Guid.NewGuid();
    var startsAtUtc = new DateTimeOffset(2026, 7, 7, 20, 0, 0, TimeSpan.Zero);
    var character = CreateCharacter(userId);
    character.ReceiveExperience(90);
    var hunt = CreateHunt(character, startsAtUtc);
    var characterRepository = new FakeCharacterRepository { CharacterToReturn = character };
    var huntRepository = new FakeHuntRepository { HuntToReturn = hunt };
    var unitOfWork = new FakeUnitOfWork();
    var clock = new FakeClock { UtcNow = startsAtUtc.AddSeconds(60) };
    var dispatcher = new RecordingDomainEventDispatcher(
      _ => Assert.Equal(1, unitOfWork.SaveChangesCallCount)
    );
    var handler = CreateHandler(characterRepository, huntRepository, unitOfWork, clock, dispatcher);

    var result = await CompleteHuntAsync(handler, userId, hunt.Id);
    var retryResult = await CompleteHuntAsync(handler, userId, hunt.Id);

    Assert.True(result.IsSuccess);
    Assert.Equal(result, retryResult);
    var domainEvent = Assert.IsType<CharacterLeveledUp>(Assert.Single(dispatcher.DispatchedEvents));
    Assert.Equal(character.Id, domainEvent.CharacterId);
    Assert.Equal(1, domainEvent.PreviousLevel);
    Assert.Equal(2, domainEvent.NewLevel);
    Assert.Equal(115, domainEvent.TotalExperience);
    Assert.Equal(115, character.Experience);
    Assert.Equal(15, character.Gold);
    Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    Assert.Empty(character.DomainEvents);
  }

  [Fact]
  public async Task HandleAsync_Should_Not_Dispatch_Events_When_Save_Fails()
  {
    var userId = Guid.NewGuid();
    var startsAtUtc = new DateTimeOffset(2026, 7, 7, 20, 0, 0, TimeSpan.Zero);
    var character = CreateCharacter(userId);
    character.ReceiveExperience(90);
    var hunt = CreateHunt(character, startsAtUtc);
    var characterRepository = new FakeCharacterRepository { CharacterToReturn = character };
    var huntRepository = new FakeHuntRepository { HuntToReturn = hunt };
    var failure = new ConcurrencyConflictException("Concurrent update detected.");
    var unitOfWork = new FakeUnitOfWork { ConcurrencyConflictToThrow = failure };
    var clock = new FakeClock { UtcNow = startsAtUtc.AddSeconds(60) };
    var dispatcher = new RecordingDomainEventDispatcher();
    var handler = CreateHandler(characterRepository, huntRepository, unitOfWork, clock, dispatcher);

    var exception = await Assert.ThrowsAsync<ConcurrencyConflictException>(
      () => CompleteHuntAsync(handler, userId, hunt.Id)
    );

    Assert.Same(failure, exception);
    Assert.Single(character.DomainEvents);
    Assert.Empty(dispatcher.DispatchedEvents);
  }

  [Fact]
  public async Task HandleAsync_Should_Return_Success_When_Dispatch_Fails_After_Saving()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var startsAtUtc = new DateTimeOffset(2026, 7, 7, 20, 0, 0, TimeSpan.Zero);
    var character = CreateCharacter(userId);
    character.ReceiveExperience(90);
    var hunt = CreateHunt(character, startsAtUtc);

    var characterRepository = new FakeCharacterRepository
    {
      CharacterToReturn = character
    };

    var huntRepository = new FakeHuntRepository
    {
      HuntToReturn = hunt
    };

    var unitOfWork = new FakeUnitOfWork();

    var clock = new FakeClock
    {
      UtcNow = startsAtUtc.AddSeconds(60)
    };

    var dispatchAttempted = false;

    var dispatcher = new RecordingDomainEventDispatcher(_ =>
    {
      dispatchAttempted = true;

      throw new InvalidOperationException("Event handler failed.");
    });

    var handler = CreateHandler(
      characterRepository,
      huntRepository,
      unitOfWork,
      clock,
      dispatcher
    );

    // Act

    var result = await CompleteHuntAsync(
      handler,
      userId,
      hunt.Id
    );

    // Assert

    Assert.True(result.IsSuccess);

    Assert.True(dispatchAttempted);

    Assert.Equal(1, unitOfWork.SaveChangesCallCount);

    Assert.Equal(HuntStatus.Completed, hunt.Status);

    Assert.Equal(115, character.Experience);

    Assert.Equal(15, character.Gold);

    Assert.Equal(2, character.Level);

    Assert.Empty(character.DomainEvents);
  }

  [Fact]
  public async Task HandleAsync_Should_Return_CharacterNotFound_When_Character_Does_Not_Exist()
  {
    var userId = Guid.NewGuid();
    var huntId = Guid.NewGuid();
    var characterRepository = new FakeCharacterRepository();
    var huntRepository = new FakeHuntRepository();
    var unitOfWork = new FakeUnitOfWork();
    var clock = new FakeClock();
    var handler = CreateHandler(characterRepository, huntRepository, unitOfWork, clock);
    var result = await CompleteHuntAsync(handler, userId, huntId);

    Assert.False(result.IsSuccess);
    Assert.Equal(CompleteHuntError.CharacterNotFound, result.Error);
    Assert.Equal(userId, characterRepository.RequestedUserIdForUpdate);
    Assert.Null(huntRepository.RequestedHuntId);
    Assert.Equal(0, unitOfWork.SaveChangesCallCount);
  }

  [Fact]
  public async Task HandleAsync_Should_Return_HuntNotFound_When_Hunt_Does_Not_Exist_Or_Belongs_To_Another_Character()
  {
    var userId = Guid.NewGuid();
    var huntId = Guid.NewGuid();
    var character = CreateCharacter(userId);

    var characterRepository = new FakeCharacterRepository
    {
      CharacterToReturn = character
    };

    var huntRepository = new FakeHuntRepository();
    var unitOfWork = new FakeUnitOfWork();
    var clock = new FakeClock();

    var handler = CreateHandler(characterRepository, huntRepository, unitOfWork, clock);

    var result = await CompleteHuntAsync(handler, userId, huntId);

    Assert.False(result.IsSuccess);
    Assert.Equal(CompleteHuntError.HuntNotFound, result.Error);
    Assert.Equal(huntId, huntRepository.RequestedHuntId);
    Assert.Equal(0, character.Experience);
    Assert.Equal(0, character.Gold);
    Assert.Equal(0, unitOfWork.SaveChangesCallCount);
  }

  private static Character CreateCharacter(Guid userId)
  {
    return Character.Create(
      userId,
      "Damon Salvatore",
      Faction.Vampire,
      new DateTimeOffset(2026, 7, 7, 19, 0, 0, TimeSpan.Zero)
    );
  }

  private static Hunt CreateHunt(Character character, DateTimeOffset startsAtUtc)
  {
    return Hunt.Start(
      character.Id,
      "shadow_hunt",
      startsAtUtc,
      TimeSpan.FromSeconds(60),
      energyCost: 10,
      experienceReward: 25,
      goldReward: 15
    );
  }

  private static CompleteHuntHandler CreateHandler(
    FakeCharacterRepository characterRepository,
    FakeHuntRepository huntRepository,
    FakeUnitOfWork unitOfWork,
    FakeClock clock,
    IDomainEventDispatcher? eventDispatcher = null
  )
  {
    return new CompleteHuntHandler(
      characterRepository,
      huntRepository,
      unitOfWork,
      clock,
      eventDispatcher ?? new RecordingDomainEventDispatcher(),
      NullLogger<CompleteHuntHandler>.Instance
    );
  }

  private static Task<CompleteHuntResult> CompleteHuntAsync(
    CompleteHuntHandler handler,
    Guid userId,
    Guid huntId
  )
  {
    return handler.HandleAsync(
      new CompleteHuntCommand(
        UserId: userId,
        HuntId: huntId
      ),
      CancellationToken.None
    );
  }

  [Fact]
  public async Task HandleAsync_Should_Return_Success_When_Another_Request_CompletedHuntFirst()
  {
    var userId = Guid.NewGuid();
    var starsAtUtc = new DateTimeOffset(2026, 7, 22, 20, 0, 0, TimeSpan.Zero);
    var completedAtUtc = starsAtUtc.AddSeconds(60);
    var character = CreateCharacter(userId);
    var staleHunt = Hunt.Start(
      characterId: character.Id,
      definitionCode: "shadow_hunt",
      startsAtUtc: starsAtUtc,
      duration: TimeSpan.FromSeconds(60),
      energyCost: 10,
      experienceReward: 25,
      goldReward: 15
    );

    var persistedHunt = Hunt.Start(
      characterId: character.Id,
      definitionCode: "shadow_hunt",
      startsAtUtc: starsAtUtc,
      duration: TimeSpan.FromSeconds(60),
      energyCost: 10,
      experienceReward: 25,
      goldReward: 15
    );

    persistedHunt.Complete(completedAtUtc);

    var characterRepository = new FakeCharacterRepository
    {
      CharacterToReturn = character
    };

    var huntRepository = new FakeHuntRepository
    {
      HuntToReturn = staleHunt,
      HuntSnapshotToReturn = persistedHunt
    };

    var unitOfWork = new FakeUnitOfWork
    {
      ConcurrencyConflictToThrow = new ConcurrencyConflictException(
        "Concurrent completion detected. Another request has already completed the hunt."
      )
    };

    var clock = new FakeClock
    {
      UtcNow = completedAtUtc.AddSeconds(1)
    };

    var handler = CreateHandler(characterRepository, huntRepository, unitOfWork, clock);

    var result = await CompleteHuntAsync(handler, userId, staleHunt.Id);

    Assert.True(result.IsSuccess);

    Assert.Equal(completedAtUtc, persistedHunt.CompletedAtUtc);
    Assert.Equal(completedAtUtc, result.CompletedAtUtc);

    Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    Assert.Equal(1, huntRepository.GetSnapshotCallCount);
  }

  private sealed class RecordingDomainEventDispatcher(
    Action<IDomainEvent>? onDispatch = null
  ) : IDomainEventDispatcher
  {
    public List<IDomainEvent> DispatchedEvents { get; } = [];

    public Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
      onDispatch?.Invoke(domainEvent);
      DispatchedEvents.Add(domainEvent);
      return Task.CompletedTask;
    }
  }

  private sealed class TrackingUnitOfWork(List<string> executionOrder) : IUnitOfWork
  {
    private readonly List<string> _executionOrder = executionOrder;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
      _executionOrder.Add(nameof(SaveChangesAsync));

      return Task.FromResult(1);
    }
  }

  private sealed class TrackingDomainEventDispatcher(
    List<string> executionOrder
    ) : IDomainEventDispatcher
  {
    private readonly List<string> _executionOrder = executionOrder;

    public List<IDomainEvent> ReceivedEvents { get; } = [];

    public Task DispatchAsync(
      IDomainEvent domainEvent,
      CancellationToken cancellationToken = default
    )
    {
      _executionOrder.Add("Dispatch");

      return Task.CompletedTask;
    }
  }
}

using Bloodline.Application.Abstractions.Events;
using Bloodline.Domain.Characters.Events;
using Bloodline.Infrastructure.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Bloodline.Infrastructure.Tests.Events;

public class InProcessDomainEventDispatcherTests
{
  [Fact]
  public async Task DispatchAsync_Should_Invoke_Registered_Handler()
  {
    // Arrange
    var receivedEvents = new List<CharacterLeveledUp>();

    var services = new ServiceCollection();

    services.AddScoped<IDomainEventHandler<CharacterLeveledUp>>(
      _ => new RecordingLevelUpHandler(receivedEvents)
    );

    services.AddScoped<IDomainEventDispatcher, InProcessDomainEventDispatcher>();

    using var provider = services.BuildServiceProvider(validateScopes: true);

    using var scope = provider.CreateScope();

    var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();

    var domainEvent = new CharacterLeveledUp(
      Guid.NewGuid(),
      1,
      2,
      100L
    );

    // Act
    await dispatcher.DispatchAsync(domainEvent);

    // Assert

    var receivedEvent = receivedEvents.Single();

    Assert.Same(domainEvent, receivedEvent);
  }

  private sealed class RecordingLevelUpHandler(List<CharacterLeveledUp> receiveEvents) : IDomainEventHandler<CharacterLeveledUp>
  {
    private readonly List<CharacterLeveledUp> _receiveEvents = receiveEvents;

    public Task HandleAsync(
      CharacterLeveledUp domainEvent,
      CancellationToken cancellationToken = default
    )
    {
      _receiveEvents.Add(domainEvent);
      return Task.CompletedTask;
    }
  }
}
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

  [Fact]
  public async Task DispatchAsync_Should_Invoke_All_Registered_Handlers()
  {
    // Arrange
    var firstReceivedEvents = new List<CharacterLeveledUp>();
    var secondReceivedEvents = new List<CharacterLeveledUp>();

    var services = new ServiceCollection();

    services.AddScoped<IDomainEventHandler<CharacterLeveledUp>>(
      _ => new RecordingLevelUpHandler(firstReceivedEvents)
    );

    services.AddScoped<IDomainEventHandler<CharacterLeveledUp>>(
      _ => new RecordingLevelUpHandler(secondReceivedEvents)
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
    var firstReceivedEvent = firstReceivedEvents.Single();
    var secondReceivedEvent = secondReceivedEvents.Single();

    Assert.Same(domainEvent, firstReceivedEvent);
    Assert.Same(domainEvent, secondReceivedEvent);
  }

  private static readonly string[] expected = ["first", "failing"];

  [Fact]
  public async Task DispatchAsync_Should_Stop_And_Propagate_When_Handler_Fails()
  {
    // Arrange
    var executions = new List<string>();

    var services = new ServiceCollection();

    AddTrackingHandler(services, "first", executions);

    services.AddScoped<IDomainEventHandler<CharacterLeveledUp>>(
        _ => new FailingLevelUpHandler(executions)
    );

    AddTrackingHandler(services, "third", executions);

    services.AddScoped<
        IDomainEventDispatcher,
        InProcessDomainEventDispatcher
    >();

    using var provider = services.BuildServiceProvider(
        validateScopes: true
    );

    using var scope = provider.CreateScope();

    var dispatcher = scope.ServiceProvider
        .GetRequiredService<IDomainEventDispatcher>();

    var domainEvent = new CharacterLeveledUp(
        Guid.NewGuid(),
        1,
        2,
        100L
    );

    // Act
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(
        () => dispatcher.DispatchAsync(domainEvent)
    );

    // Assert
    Assert.Equal(
        "Simulated handler failure",
        exception.Message
    );

    Assert.Equal(
        expected,
        executions
    );
  }

  [Fact]
  public async Task DispatchAsync_Should_Propagate_Original_Exception_When_Handler_Throws_Synchronously()
  {
    // Arrange
    var services = new ServiceCollection();

    services.AddScoped<IDomainEventHandler<CharacterLeveledUp>>(
        _ => new SynchronousFailingLevelUpHandler()
    );

    services.AddScoped<
        IDomainEventDispatcher,
        InProcessDomainEventDispatcher
    >();

    using var provider = services.BuildServiceProvider(
        validateScopes: true
    );

    using var scope = provider.CreateScope();

    var dispatcher = scope.ServiceProvider
        .GetRequiredService<IDomainEventDispatcher>();

    var domainEvent = new CharacterLeveledUp(
        Guid.NewGuid(),
        1,
        2,
        100L
    );

    // Act
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(
        () => dispatcher.DispatchAsync(domainEvent)
    );

    // Assert
    Assert.Equal(
        "Simulated synchronous handler failure",
        exception.Message
    );
  }

  private sealed class SynchronousFailingLevelUpHandler : IDomainEventHandler<CharacterLeveledUp>
  {
    public Task HandleAsync(
      CharacterLeveledUp domainEvent,
      CancellationToken cancellationToken = default
    )
    {
      throw new InvalidOperationException(
        "Simulated synchronous handler failure"
      );
    }
  }

  private static void AddTrackingHandler(
    IServiceCollection services,
    string name,
    List<string> executions
  )
  {
    services.AddScoped<IDomainEventHandler<CharacterLeveledUp>>(
      _ => new TrackingLevelUpHandler(name, executions)
    );
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

  private sealed class TrackingLevelUpHandler(
      string name,
      List<string> executions) : IDomainEventHandler<CharacterLeveledUp>
  {
    private readonly string _name = name;
    private readonly List<string> _executions = executions;

    public Task HandleAsync(
        CharacterLeveledUp domainEvent,
        CancellationToken cancellationToken = default)
    {
      _executions.Add(_name);

      return Task.CompletedTask;
    }
  }

  private sealed class FailingLevelUpHandler(List<string> executions) : IDomainEventHandler<CharacterLeveledUp>
  {
    private readonly List<string> _executions = executions;

    public Task HandleAsync(
        CharacterLeveledUp domainEvent,
        CancellationToken cancellationToken = default)
    {
      _executions.Add("failing");

      return Task.FromException(
        new InvalidOperationException("Simulated handler failure")
      );
    }
  }
}
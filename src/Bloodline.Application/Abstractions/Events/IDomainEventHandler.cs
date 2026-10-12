using Bloodline.Domain.Common;

namespace Bloodline.Application.Abstractions.Events;

public interface IDomainEventHandler<TEvent> where TEvent : IDomainEvent
{
  Task HandleAsync(
    TEvent domainEvent,
    CancellationToken cancellationToken = default
  );
}
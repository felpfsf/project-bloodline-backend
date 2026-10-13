using Bloodline.Domain.Common;

namespace Bloodline.Application.Abstractions.Events;

public interface IDomainEventDispatcher
{
  Task DispatchAsync(
    IDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  );
}
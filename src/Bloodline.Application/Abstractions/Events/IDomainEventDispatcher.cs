using Bloodline.Domain.Common;

namespace Bloodline.Application.Abstractions.Events;

public interface IDomainEventDispatcher
{
  Task DispatchAsync<TDomainEvent>(
    IDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  );
}
using System.Reflection;
using System.Runtime.ExceptionServices;
using Bloodline.Application.Abstractions.Events;
using Bloodline.Domain.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Bloodline.Infrastructure.Events;

public sealed class InProcessDomainEventDispatcher(
    IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task DispatchAsync(
        IDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var handlerType = typeof(IDomainEventHandler<>)
            .MakeGenericType(domainEvent.GetType());

        var handlers = _serviceProvider.GetServices(handlerType);

        var handleMethod = handlerType.GetMethod("HandleAsync")
            ?? throw new InvalidOperationException(
                "Domain event handler method not found."
            );

        foreach (var handler in handlers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            object? result;

            try
            {
                result = handleMethod.Invoke(
                    handler,
                    [domainEvent, cancellationToken]
                );
            }
            catch (TargetInvocationException ex) 
                when (ex.InnerException is not null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }

            if (result is not Task task)
            {
                throw new InvalidOperationException(
                    "Domain event handler must return a Task."
                );
            }

            await task;
        }
    }
}
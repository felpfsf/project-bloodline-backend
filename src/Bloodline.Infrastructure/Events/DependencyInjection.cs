using Bloodline.Application.Abstractions.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Bloodline.Infrastructure.Events;

public static class DependencyInjection
{
  public static IServiceCollection AddDomainEventDispatching(
      this IServiceCollection services)
  {
    services.AddScoped<
        IDomainEventDispatcher,
        InProcessDomainEventDispatcher
    >();

    return services;
  }
}
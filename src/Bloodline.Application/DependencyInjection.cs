using Bloodline.Application.Identity.Register;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Bloodline.Application;

public static class DependencyInjection
{
  public static IServiceCollection AddApplication(
    this IServiceCollection services
  )
  {
    services.AddScoped<RegisterUserHandler>();

    services.AddScoped<IValidator<RegisterUserCommand>, RegisterUserCommandValidator>();

    return services;
  }
}
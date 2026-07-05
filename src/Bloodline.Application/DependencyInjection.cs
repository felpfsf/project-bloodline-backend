using Bloodline.Application.Characters.CreateCharacter;
using Bloodline.Application.Characters.GetCurrentCharacter;
using Bloodline.Application.Identity.Login;
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
    services.AddScoped<LoginUserHandler>();
    services.AddScoped<CreateCharacterHandler>();
    services.AddScoped<GetCurrentCharacterHandler>();

    services.AddScoped<IValidator<RegisterUserCommand>, RegisterUserCommandValidator>();
    services.AddScoped<IValidator<LoginCommand>, LoginUserCommandValidator>();
    services.AddScoped<IValidator<CreateCharacterCommand>, CreateCharacterCommandValidator>();

    return services;
  }
}
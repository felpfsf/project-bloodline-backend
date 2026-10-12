using Bloodline.Application.Abstractions.Events;
using Bloodline.Application.Characters.CreateCharacter;
using Bloodline.Application.Characters.EventHandlers;
using Bloodline.Application.Characters.GetCurrentCharacter;
using Bloodline.Application.Hunts.CompleteHunt;
using Bloodline.Application.Hunts.GetActiveHunt;
using Bloodline.Application.Hunts.StartHunt;
using Bloodline.Application.Identity.Login;
using Bloodline.Application.Identity.Logout;
using Bloodline.Application.Identity.Register;
using Bloodline.Domain.Characters.Events;
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
    services.AddScoped<LogoutHandler>();

    services.AddScoped<CreateCharacterHandler>();
    services.AddScoped<GetCurrentCharacterHandler>();

    services.AddScoped<StartHuntHandler>();
    services.AddScoped<GetActiveHuntHandler>();
    services.AddScoped<CompleteHuntHandler>();

    services.AddScoped<IValidator<RegisterUserCommand>, RegisterUserCommandValidator>();
    services.AddScoped<IValidator<LoginCommand>, LoginUserCommandValidator>();
    services.AddScoped<IValidator<CreateCharacterCommand>, CreateCharacterCommandValidator>();

    services.AddScoped<IDomainEventHandler<CharacterLevelUp>, LogCharacterLevelUpHandler>();

    return services;
  }
}
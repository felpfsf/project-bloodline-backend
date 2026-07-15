using Bloodline.Application.Characters.Abstractions;
using Bloodline.Application.Common.Persistence;
using Bloodline.Application.Common.Time;
using Bloodline.Application.Hunts.Abstractions;
using Bloodline.Application.Identity.Abstractions;
using Bloodline.Infrastructure.Cache;
using Bloodline.Infrastructure.Characters.Persistence;
using Bloodline.Infrastructure.Common.Time;
using Bloodline.Infrastructure.Hunts.Persistence;
using Bloodline.Infrastructure.Identity.Persistence;
using Bloodline.Infrastructure.Identity.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Bloodline.Infrastructure.Persistence;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    var connectionString =
        configuration.GetConnectionString("PostgreSql")
        ?? throw new InvalidOperationException("Connection string 'PostgreSql' not found or configuration is invalid.");

    services.AddDbContext<BloodlineDbContext>(options => options.UseNpgsql(connectionString));

    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<ICharacterRepository, CharacterRepository>();
    services.AddScoped<IHuntRepository, HuntRepository>();

    services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<BloodlineDbContext>());

    services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

    services.AddSingleton<IPasswordHasher, AspNetPasswordHasher>();
    services.AddSingleton<ITokenService, JwtTokenService>();
    services.AddSingleton<IClock, SystemClock>();
    services.AddSingleton(TimeProvider.System);

    var redisConnectionString =
        configuration.GetConnectionString("Redis")
        ?? throw new InvalidOperationException("Connection string 'Redis' not found or configuration is invalid.");

    services.AddSingleton<IConnectionMultiplexer>(
        ConnectionMultiplexer.Connect(redisConnectionString)
    );
    services.AddSingleton<ITokenBlacklist, RedisTokenBlacklist>();

    services.AddOptions<JwtOptions>()
        .Bind(configuration.GetSection(JwtOptions.SectionName))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    return services;
  }
}
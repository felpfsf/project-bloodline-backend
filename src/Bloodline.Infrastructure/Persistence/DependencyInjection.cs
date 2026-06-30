using Bloodline.Application.Common.Time;
using Bloodline.Application.Identity.Abstractions;
using Bloodline.Infrastructure.Common.Time;
using Bloodline.Infrastructure.Identity.Persistence;
using Bloodline.Infrastructure.Identity.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

    services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

    services.AddSingleton<IPasswordHasher, AspNetPasswordHasher>();
    services.AddSingleton<ITokenService, JwtTokenService>();
    services.AddSingleton<IClock, SystemClock>();
    services.AddSingleton(TimeProvider.System);

    services.AddOptions<JwtOptions>()
        .Bind(configuration.GetSection(JwtOptions.SectionName))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    return services;
  }
}
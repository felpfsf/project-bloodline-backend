using Bloodline.Application.Common.Time;
using Bloodline.Application.Identity.Abstractions;
using Bloodline.Infrastructure.Common.Time;
using Bloodline.Infrastructure.Identity.Persistence;
using Bloodline.Infrastructure.Identity.Security;
using Bloodline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bloodline.Infrastructure;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    var connectionString = 
        configuration.GetConnectionString("PostgreSql") 
        ?? throw new InvalidOperationException("Connection string 'PostgreSql' not found or configuration is invalid.");

    services.AddDbContext<BloodlineDbContext>(options => options.UseNpgsql(connectionString));

    services.AddScoped<IUserRepository, UserRepository>();

    services.AddSingleton<IPasswordHasher, AspNetPasswordHasher>();

    services.AddSingleton<IClock, SystemClock>();

    return services;
  }
}
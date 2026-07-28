using Bloodline.Infrastructure.Persistence;
using Bloodline.Application.Identity.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Bloodline.Application.Common.Time;

namespace Bloodline.IntegrationTests.Infrastructure;

public sealed class BloodlineWebApplicationFactory(string connectionString) : WebApplicationFactory<Program>
{
  private readonly string _connectionString = connectionString;

  public IntegrationTestClock TestClock { get; } = new();

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.UseEnvironment("Testing");

    builder.ConfigureServices(services =>
    {
      var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BloodlineDbContext>));
      if (descriptor != null) services.Remove(descriptor);

      services.AddDbContext<BloodlineDbContext>(options => options.UseNpgsql(_connectionString));

      var tokenBlacklistDescriptor = services.Single(d => d.ServiceType == typeof(ITokenBlacklist));
      services.Remove(tokenBlacklistDescriptor);
      services.AddSingleton<ITokenBlacklist, TestTokenBlacklist>();

      services.RemoveAll<IClock>();

      services.AddSingleton<IClock>(TestClock);
    });
  }

  private sealed class TestTokenBlacklist : ITokenBlacklist
  {
    public Task BlacklistAsync(string jti, DateTimeOffset expiresAtUtc, CancellationToken cancellationToken) =>
      Task.CompletedTask;

    public Task<bool> IsBlacklistedAsync(string jti, CancellationToken cancellationToken) =>
      Task.FromResult(false);
  }
}
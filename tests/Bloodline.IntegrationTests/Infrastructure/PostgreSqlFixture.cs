using Bloodline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Bloodline.IntegrationTests.Infrastructure;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
  private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:17-alpine").Build();

  public BloodlineWebApplicationFactory Factory { get; private set; } = null!;

  public async Task InitializeAsync()
  {
    await _database.StartAsync();
    Factory = new BloodlineWebApplicationFactory(_database.GetConnectionString());

    using var scope = Factory.Services.CreateScope();

    var dbContext = scope.ServiceProvider.GetRequiredService<BloodlineDbContext>();

    await dbContext.Database.MigrateAsync();
  }

  public async Task DisposeAsync()
  {
    await Factory.DisposeAsync();
    await _database.StopAsync();
  }
}
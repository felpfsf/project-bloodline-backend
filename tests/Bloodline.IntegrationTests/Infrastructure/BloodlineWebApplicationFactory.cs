using Bloodline.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bloodline.IntegrationTests.Infrastructure;

public sealed class BloodlineWebApplicationFactory(string connectionString) : WebApplicationFactory<Program>
{
  private readonly string _connectionString = connectionString;

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.UseEnvironment("Testing");

    builder.ConfigureServices(services =>
    {
      var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BloodlineDbContext>));
      if (descriptor != null) services.Remove(descriptor);

      services.AddDbContext<BloodlineDbContext>(options => options.UseNpgsql(_connectionString));
    });
  }
}
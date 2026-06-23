using Bloodline.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bloodline.Infrastructure.Persistence;

public sealed class BloodlineDbContext(DbContextOptions<BloodlineDbContext> options) : DbContext(options)
{
  public DbSet<User> Users => Set<User>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(BloodlineDbContext).Assembly);
  }

}
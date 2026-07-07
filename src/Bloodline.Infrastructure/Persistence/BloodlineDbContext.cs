using Bloodline.Application.Common.Persistence;
using Bloodline.Domain.Characters;
using Bloodline.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bloodline.Infrastructure.Persistence;

public sealed class BloodlineDbContext(DbContextOptions<BloodlineDbContext> options) : DbContext(options), IUnitOfWork
{
  public DbSet<User> Users => Set<User>();
  public DbSet<Character> Characters => Set<Character>();

  async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
  {
    await SaveChangesAsync(cancellationToken);
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(BloodlineDbContext).Assembly);
  }
}
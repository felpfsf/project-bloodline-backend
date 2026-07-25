using Bloodline.Application.Common.Persistence;
using Bloodline.Domain.Characters;
using Bloodline.Domain.Hunts;
using Bloodline.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bloodline.Infrastructure.Persistence;

public sealed class BloodlineDbContext(DbContextOptions<BloodlineDbContext> options) : DbContext(options), IUnitOfWork
{
  public DbSet<User> Users => Set<User>();
  public DbSet<Character> Characters => Set<Character>();

  public DbSet<Hunt> Hunts => Set<Hunt>();

  public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      return base.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateConcurrencyException exception)
    {

      throw new ConcurrencyConflictException(
        "A concurrent persistence update was detected. The entity has been modified by another process.",
        exception
      );
    }
    catch (DbUpdateException exception)
    {
      throw new Exception("An error occurred while saving changes to the database.", exception);
    }
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(BloodlineDbContext).Assembly);
  }
}
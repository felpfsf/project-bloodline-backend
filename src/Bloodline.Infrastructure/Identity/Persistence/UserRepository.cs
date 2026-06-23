using Bloodline.Application.Identity.Abstractions;
using Bloodline.Domain.Identity;
using Bloodline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bloodline.Infrastructure.Identity.Persistence;

public sealed class UserRepository(BloodlineDbContext dbContext) : IUserRepository
{
  private readonly BloodlineDbContext _dbContext = dbContext;

  public async Task CreateAsync(User user, CancellationToken cancellationToken)
  {
    _dbContext.Users.Add(user);

    await _dbContext.SaveChangesAsync(cancellationToken);
  }

  public Task<bool> ExistsByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
  {
    return _dbContext.Users.AnyAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);
  }
}
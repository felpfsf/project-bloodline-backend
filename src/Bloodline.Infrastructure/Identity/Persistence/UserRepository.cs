using Bloodline.Application.Common.Persistence;
using Bloodline.Application.Identity.Abstractions;
using Bloodline.Domain.Identity;
using Bloodline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Bloodline.Infrastructure.Identity.Persistence;

public sealed class UserRepository(BloodlineDbContext dbContext) : IUserRepository
{
  private const string NormalizedEmailUniqueConstraint = "ux_users_normalized_email";
  private readonly BloodlineDbContext _dbContext = dbContext;

  public async Task CreateAsync(User user, CancellationToken cancellationToken)
  {
    _dbContext.Users.Add(user);

    try
    {
      await _dbContext.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateException exception)
      when (exception.InnerException is PostgresException postgresException &&
            postgresException.SqlState == PostgresErrorCodes.UniqueViolation &&
            postgresException.ConstraintName == NormalizedEmailUniqueConstraint)
    {
      {
        throw new UniqueConstraintViolationException(NormalizedEmailUniqueConstraint, exception);
      }
    }
  }

  public Task<bool> ExistsByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
  {
    return _dbContext.Users.AnyAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);
  }
}
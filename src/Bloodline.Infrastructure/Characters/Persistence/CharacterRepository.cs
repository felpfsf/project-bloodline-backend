using Bloodline.Application.Characters.Abstractions;
using Bloodline.Application.Common.Persistence;
using Bloodline.Domain.Characters;
using Bloodline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Bloodline.Infrastructure.Characters.Persistence;

public sealed class CharacterRepository(BloodlineDbContext dbContext) : ICharacterRepository
{
  private const string UserIdUniqueConstraint = "ux_characters_user_id";
  private const string NormalizedNameUniqueConstraint = "ux_characters_normalized_name";
  private readonly BloodlineDbContext _dbcontext = dbContext;

  public async Task CreateAsync(Character character, CancellationToken cancellationToken)
  {
    _dbcontext.Characters.Add(character);

    try
    {
      await _dbcontext.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateException exception)
      when (exception.InnerException is PostgresException postgresException &&
            postgresException.SqlState == PostgresErrorCodes.UniqueViolation &&
            postgresException.ConstraintName == NormalizedNameUniqueConstraint)
    {
      throw new UniqueConstraintViolationException(
        NormalizedNameUniqueConstraint,
        exception
      );
    }
  }

  public Task<bool> ExistsByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken)
  {
    return _dbcontext.Characters.AnyAsync(
      character => character.NormalizedName == normalizedName,
      cancellationToken
    );
  }

  public Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken)
  {
    return _dbcontext.Characters.AnyAsync(
      character => character.UserId == userId,
      cancellationToken
    );
  }
}
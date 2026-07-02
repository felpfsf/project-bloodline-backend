using Bloodline.Application.Characters.Abstractions;
using Bloodline.Application.Common.Persistence;
using Bloodline.Domain.Characters;

namespace Bloodline.Application.Tests.Characters.Fakes;

internal sealed class FakeCharacterRepository : ICharacterRepository
{
  public bool UserAlreadyHasCharacter { get; set; }
  public bool NameAlreadyTaken { get; set; }
  public string? CheckedNormalizedName { get; private set; }
  public Guid? CheckedUserId { get; private set; }
  public Character? CreatedCharacter { get; private set; }
  public string? UniqueConstraintToThrow { get; set; }

  public Task CreateAsync(Character character, CancellationToken cancellationToken)
  {
    if (UniqueConstraintToThrow is not null)
    {
      throw new UniqueConstraintViolationException(
        UniqueConstraintToThrow,
        new Exception("Unique constraint violation exception for testing purposes")
      );
    }

    CreatedCharacter = character;

    return Task.CompletedTask;
  }

  public Task<bool> ExistsByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken)
  {
    CheckedNormalizedName = normalizedName;
    return Task.FromResult(NameAlreadyTaken);
  }

  public Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken)
  {
    CheckedUserId = userId;
    return Task.FromResult(UserAlreadyHasCharacter);
  }
}
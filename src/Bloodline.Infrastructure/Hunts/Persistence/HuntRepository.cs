using Bloodline.Application.Hunts.Abstractions;
using Bloodline.Domain.Hunts;
using Bloodline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bloodline.Infrastructure.Hunts.Persistence;

public sealed class HuntRepository(BloodlineDbContext context) : IHuntRepository
{
  private readonly BloodlineDbContext _dbContext = context;

  public Task<bool> ExistsByCharacterIdAsync(Guid characterId, CancellationToken cancellationToken)
  {
    return _dbContext.Hunts
      .AsNoTracking()
      .AnyAsync(
        hunt =>
          hunt.CharacterId == characterId &&
          hunt.Status == HuntStatus.Active,
        cancellationToken
      );
  }

  public Task<Hunt?> GetActiveByCharacterIdAsync(Guid characterId, CancellationToken cancellationToken)
  {
    return _dbContext.Hunts
      .AsNoTracking()
      .SingleOrDefaultAsync(
        hunt =>
          hunt.CharacterId == characterId &&
          hunt.Status == HuntStatus.Active,
        cancellationToken
      );
  }


  public Task<Hunt?> GetByIdAndCharacterIdAsync(Guid huntId, Guid characterId, CancellationToken cancellationToken)
  {
    return _dbContext.Hunts
      .AsNoTracking()
      .SingleOrDefaultAsync(
        hunt =>
          hunt.Id == huntId &&
          hunt.CharacterId == characterId,
        cancellationToken
      );
  }

  public async Task CreateAsync(Hunt hunt, CancellationToken cancellationToken)
  {
    await _dbContext.Hunts.AddAsync(
      hunt,
      cancellationToken
    );
  }


}
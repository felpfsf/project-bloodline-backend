using Bloodline.Application.Identity.Abstractions;
using StackExchange.Redis;

namespace Bloodline.Infrastructure.Cache;

public sealed class RedisTokenBlacklist(IConnectionMultiplexer redis) : ITokenBlacklist
{
  private const string KeyPrefix = "blacklist:token:";
  private readonly IDatabase _db = redis.GetDatabase();

  public async Task BlacklistAsync(string jti, DateTimeOffset expiresAtUtc, CancellationToken cancellationToken)
  {
    var key = KeyPrefix + jti;
    var ttl = expiresAtUtc - DateTimeOffset.UtcNow;

    if (ttl <= TimeSpan.Zero)
      return;

    await _db.StringSetAsync(key, "1", ttl);
  }

  public async Task<bool> IsBlacklistedAsync(string jti, CancellationToken cancellationToken)
  {
    var key = KeyPrefix + jti;
    return await _db.KeyExistsAsync(key);
  }
}

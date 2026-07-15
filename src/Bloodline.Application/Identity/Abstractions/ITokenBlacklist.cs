namespace Bloodline.Application.Identity.Abstractions;

public interface ITokenBlacklist
{
  Task BlacklistAsync(string jti, DateTimeOffset expiresAtUtc, CancellationToken cancellationToken);
  Task<bool> IsBlacklistedAsync(string jti, CancellationToken cancellationToken);
}

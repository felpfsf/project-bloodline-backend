using Bloodline.Application.Identity.Abstractions;

namespace Bloodline.Application.Identity.Logout;

public sealed class LogoutHandler(ITokenBlacklist tokenBlacklist)
{
  private readonly ITokenBlacklist _tokenBlacklist = tokenBlacklist;

  public async Task<LogoutResult> HandleAsync(LogoutCommand command, CancellationToken cancellationToken)
  {
    await _tokenBlacklist.BlacklistAsync(command.Jti, command.ExpiresAtUtc, cancellationToken);

    return LogoutResult.Success();
  }
}

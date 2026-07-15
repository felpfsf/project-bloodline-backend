namespace Bloodline.Application.Identity.Logout;

public sealed record LogoutCommand(
  string Jti,
  DateTimeOffset ExpiresAtUtc
);

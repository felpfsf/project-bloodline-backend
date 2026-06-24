namespace Bloodline.Api.Features.Identity.Login;

public sealed record LoginResponse(
  string AccessToken,
  DateTimeOffset ExpiresAtUtc
);
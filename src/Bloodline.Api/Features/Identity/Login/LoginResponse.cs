namespace Bloodline.Api.Features.Identity.Login;

public sealed record LoginResponse(
  Guid UserId,
  string Email
);
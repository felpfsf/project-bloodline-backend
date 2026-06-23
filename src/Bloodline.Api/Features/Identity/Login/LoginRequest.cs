namespace Bloodline.Api.Features.Identity.Login;

public sealed record LoginRequest(
  string Email,
  string Password
);
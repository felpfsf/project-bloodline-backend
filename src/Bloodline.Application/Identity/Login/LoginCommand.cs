namespace Bloodline.Application.Identity.Login;

public sealed record LoginCommand(
  string Email,
  string Password
);
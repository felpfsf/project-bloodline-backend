namespace Bloodline.Application.Identity.Register;

public sealed record RegisterUserCommand(
  string Email,
  string Password
);
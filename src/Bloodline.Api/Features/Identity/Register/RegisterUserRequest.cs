namespace Bloodline.Api.Features.Identity.Register;

public sealed record RegisterUserRequest(
  string Email,
  string Password
  );
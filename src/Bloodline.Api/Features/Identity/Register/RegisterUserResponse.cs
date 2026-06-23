namespace Bloodline.Api.Features.Identity.Register;

public sealed record RegisterUserResponse(
  Guid UserId,
  string Email,
  DateTimeOffset CreatedAtUtc
);
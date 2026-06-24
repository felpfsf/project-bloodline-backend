namespace Bloodline.Application.Identity.Models;

public sealed record AccessToken(
  string Value,
  DateTimeOffset ExpiresAtUtc
);
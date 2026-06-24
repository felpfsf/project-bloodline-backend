namespace Bloodline.Infrastructure.Identity.Security;

public sealed class JwtOptions
{
  public const string SectionName = "Jwt";
  public string Issuer { get; set; } = null!;
  public string Audience { get; set; } = null!;
  public string SigningKey { get; set; } = null!;
  public int AccessTokenExpirationMinutes { get; set; }
}
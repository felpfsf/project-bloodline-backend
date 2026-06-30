using System.ComponentModel.DataAnnotations;

namespace Bloodline.Infrastructure.Identity.Security;

public sealed class JwtOptions
{
  public const string SectionName = "Jwt";
  [Required]
  public string Issuer { get; set; } = null!;
  [Required]
  public string Audience { get; set; } = null!;
  [Required]
  [MinLength(32)]
  public string SigningKey { get; set; } = null!;
  [Range(1, 1440)]
  public int AccessTokenExpirationMinutes { get; set; }
}
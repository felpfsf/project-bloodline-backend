using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Bloodline.Application.Identity.Abstractions;
using Bloodline.Application.Identity.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Bloodline.Infrastructure.Identity.Security;

public sealed class JwtTokenService : ITokenService
{
  private readonly JwtOptions _options;
  private readonly TimeProvider _timeProvider;

  public JwtTokenService(
      IOptions<JwtOptions> options,
      TimeProvider timeProvider)
  {
    _options = options.Value;
    _timeProvider = timeProvider;
  }

  public AccessToken Generate(
      Guid userId)
  {
    var issuedAtUtc = _timeProvider.GetUtcNow();
    var expiresAtUtc = issuedAtUtc.AddMinutes(
        _options.AccessTokenExpirationMinutes);

    var claims = new[]
    {
      new Claim(
          JwtRegisteredClaimNames.Sub,
          userId.ToString()),

      new Claim(
          JwtRegisteredClaimNames.Jti,
          Guid.NewGuid().ToString()),

      new Claim(
          JwtRegisteredClaimNames.Iat,
          issuedAtUtc.ToUnixTimeSeconds().ToString(),
          ClaimValueTypes.Integer64)
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: _options.Issuer,
        audience: _options.Audience,
        claims: claims,
        notBefore: issuedAtUtc.UtcDateTime,
        expires: expiresAtUtc.UtcDateTime,
        signingCredentials: credentials);

    var value = new JwtSecurityTokenHandler().WriteToken(token);

    return new AccessToken(
        value,
        expiresAtUtc);
  }
}
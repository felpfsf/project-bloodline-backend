using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Bloodline.Application.Identity.Logout;

namespace Bloodline.Api.Features.Identity.Logout;

public static class LogoutEndpoint
{
  public static async Task<IResult> HandleAsync(
    ClaimsPrincipal user,
    LogoutHandler handler,
    CancellationToken cancellationToken
  )
  {
    var jti = user.FindFirstValue(JwtRegisteredClaimNames.Jti);

    if (string.IsNullOrEmpty(jti))
    {
      return Results.Problem(
        statusCode: StatusCodes.Status401Unauthorized,
        title: "Invalid authentication token."
      );
    }

    var expClaim = user.FindFirstValue(JwtRegisteredClaimNames.Exp);
    if (!long.TryParse(expClaim, out var expUnix))
    {
      return Results.Problem(
        statusCode: StatusCodes.Status401Unauthorized,
        title: "Invalid authentication token."
      );
    }

    var expiresAtUtc = DateTimeOffset.FromUnixTimeSeconds(expUnix);
    var command = new LogoutCommand(jti, expiresAtUtc);

    await handler.HandleAsync(command, cancellationToken);

    return Results.NoContent();
  }
}

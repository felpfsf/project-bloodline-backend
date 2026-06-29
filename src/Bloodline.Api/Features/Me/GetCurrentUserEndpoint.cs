using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Bloodline.Api.Features.Me;

public static class GetCurrentUserEndpoint
{
  public static IResult Handle(ClaimsPrincipal user)
  {
    var subject =
      user.FindFirstValue(JwtRegisteredClaimNames.Sub)
      ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

    if (!Guid.TryParse(subject, out var userId))
    {
      return Results.Problem(
        statusCode: StatusCodes.Status500InternalServerError,
        detail: "Authenticated user does not have a valid subject claim."
      );
    }

    var response = new GetCurrentUserResponse(userId);

    return Results.Ok(response);
  }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Bloodline.Api.Common.Http;

namespace Bloodline.Api.Features.Me;

public static class GetCurrentUserEndpoint
{
  public static IResult Handle(ClaimsPrincipal user)
  {
    var subject =
      user.FindFirstValue(JwtRegisteredClaimNames.Sub)
      ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

    if (!Guid.TryParse(subject, out var userId))
      return ProblemDetailsExtensions.Problem(
        StatusCodes.Status500InternalServerError,
        "Invalid authenticated user",
        "Authenticated user does not have a valid subject claim.",
        "invalid_subject_claim"
      );

    var response = new GetCurrentUserResponse(userId);

    return Results.Ok(response);
  }
}

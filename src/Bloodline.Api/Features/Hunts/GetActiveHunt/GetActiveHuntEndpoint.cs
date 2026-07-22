using System.Security.Claims;
using Bloodline.Api.Common.Http;
using Bloodline.Application.Hunts.GetActiveHunt;

namespace Bloodline.Api.Features.Hunts.GetActiveHunt;

public static class GetActiveHuntEndpoint
{
  public static async Task<IResult> HandleAsync(
    ClaimsPrincipal user,
    GetActiveHuntHandler handler,
    CancellationToken cancellationToken
  )
  {
    var userIdClaim = user.FindFirstValue("sub");

    if (!Guid.TryParse(userIdClaim, out var userId))
    {
      return Results.Problem(
        statusCode: StatusCodes.Status401Unauthorized,
        title: "Invalid authentication token."
      );
    }

    var result = await handler.HandleAsync(
      new GetActiveHuntQuery(userId),
      cancellationToken
    );

    if (!result.IsSuccess)
      return ToErrorResult(result);

    if (!result.HasActiveHunt)
      return Results.Ok(new GetActiveHuntResponse(null));

    return Results.Ok(ToResponse(result));
  }

  private static IResult ToErrorResult(GetActiveHuntResult result)
  {
    return result.Error switch
    {
      GetActiveHuntError.CharacterNotFound =>
        ProblemDetailsExtensions.NotFound(
          title: "Character not found",
          detail: "The authenticated user does not have a character yet.",
          code: GetActiveHuntHttpCodes.CharacterNotFound
        ),
      _ => ProblemDetailsExtensions.InternalServerError(
        title: "Unexpected error",
        detail: "An unexpected error occurred while processing the request.",
        code: "unexpected_error"
      )
    };
  }

  private static GetActiveHuntResponse ToResponse(GetActiveHuntResult result)
  {
    var response = new ActiveHuntResponse(
      result.HuntId!.Value,
      result.DefinitionCode!,
      result.StartsAtUtc!.Value,
      result.EndsAtUtc!.Value,
      result.EnergyCost!.Value,
      result.ExperienceReward!.Value,
      result.GoldReward!.Value
    );

    return new GetActiveHuntResponse(response);
  }
}
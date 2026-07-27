using System.Security.Claims;
using Bloodline.Api.Common.Http;
using Bloodline.Application.Hunts.CompleteHunt;

namespace Bloodline.Api.Features.Hunts.CompleteHunt;

public static class CompleteHuntEndpoint
{
  public static async Task<IResult> HandleAsync(
    Guid id,
    ClaimsPrincipal user,
    CompleteHuntHandler handler,
    CancellationToken cancellationToken
  )
  {
    var userIdClaim = user.FindFirstValue("sub");

    if (!Guid.TryParse(userIdClaim, out var userId))
      return ProblemDetailsExtensions.UnauthorizedProblem();

    var result = await handler.HandleAsync(
      new CompleteHuntCommand(userId, id),
      cancellationToken
    );

    if (!result.IsSuccess)
      return ToErrorResult(result);

    return Results.Ok(ToResponse(result));
  }

  private static IResult ToErrorResult(CompleteHuntResult result)
  {
    return result.Error switch
    {
      CompleteHuntError.CharacterNotFound =>
        ProblemDetailsExtensions.NotFound(
          title: "Character not found",
          detail: "The authenticated user does not have a character yet.",
          code: CompleteHuntHttpCodes.CharacterNotFound
        ),
      CompleteHuntError.HuntNotFound =>
        ProblemDetailsExtensions.NotFound(
          title: "Hunt not found",
          detail: "The hunt with the specified ID was not found.",
          code: CompleteHuntHttpCodes.HuntNotFound
        ),
      CompleteHuntError.HuntNotFinishedYet =>
        ProblemDetailsExtensions.ConflictProblem(
          title: "Hunt not finished yet",
          detail: "The hunt is still in progress and cannot be completed yet.",
          code: CompleteHuntHttpCodes.HuntNotFinishedYet
        ),
      _ => ProblemDetailsExtensions.UnexpectedError()
    };
  }

  private static CompleteHuntResponse ToResponse(CompleteHuntResult result)
  {
    return new CompleteHuntResponse(
      result.HuntId!.Value,
      result.DefinitionCode!,
      result.CompletedAtUtc!.Value,
      result.ExperienceReward!.Value,
      result.GoldReward!.Value
    );
  }
}
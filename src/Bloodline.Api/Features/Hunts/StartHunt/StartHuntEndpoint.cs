using System.Security.Claims;
using Bloodline.Api.Common.Http;
using Bloodline.Application.Hunts.StartHunt;

namespace Bloodline.Api.Features.Hunts.StartHunt;

public static class StartHuntEndpoint
{
  public static async Task<IResult> HandleAsync(
    ClaimsPrincipal user,
    StartHuntHandler handler,
    CancellationToken cancellationToken
  )
  {
    var userIdClaim = user.FindFirstValue("sub");

    if (!Guid.TryParse(userIdClaim, out var userId))
      return ProblemDetailsExtensions.UnauthorizedProblem();

    var command = new StartHuntCommand(userId);

    var result = await handler.HandleAsync(
      command,
      cancellationToken
    );

    if (!result.IsSuccess)
      return ToErrorResult(result);

    var response = ToResponse(result);

    return Results.Created(
      $"/api/hunts/{response.HuntId}",
      response
    );
  }

  private static IResult ToErrorResult(StartHuntResult result)
  {
    return result.Error switch
    {
      StartHuntError.CharacterNotFound =>
        ProblemDetailsExtensions.NotFound(
          title: "Character not found",
          detail: "The authenticated user does not have a character yet.",
          code: StartHuntHttpCodes.CharacterNotFound
        ),

      StartHuntError.HuntAlreadyActive =>
        ProblemDetailsExtensions.ConflictProblem(
          title: "Hunt already active",
          detail: "The character already has an active hunt.",
          code: StartHuntHttpCodes.HuntAlreadyActive
        ),
      StartHuntError.NotEnoughEnergy =>
        ProblemDetailsExtensions.ConflictProblem(
          title: "Not enough energy",
          detail: "The character does not have enough energy to start a hunt.",
          code: StartHuntHttpCodes.NotEnoughEnergy
        ),
      _ => ProblemDetailsExtensions.UnexpectedError()
    };
  }

  private static StartHuntResponse ToResponse(StartHuntResult result)
  {
    return new StartHuntResponse(
      HuntId: result.HuntId!.Value,
      DefinitionCode: result.DefinitionCode!,
      StartsAtUtc: result.StartsAtUtc!.Value,
      EndsAtUtc: result.EndsAtUtc!.Value,
      EnergyCost: result.EnergyCost!.Value,
      ExperienceReward: result.ExperienceReward!.Value,
      GoldReward: result.GoldReward!.Value,
      RemainingEnergy: result.RemainingEnergy!.Value
    );
  }
}
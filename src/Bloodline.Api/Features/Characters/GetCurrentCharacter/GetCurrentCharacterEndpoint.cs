using System.Security.Claims;
using Bloodline.Api.Common.Http;
using Bloodline.Application.Characters.GetCurrentCharacter;
using Bloodline.Domain.Characters;

namespace Bloodline.Api.Features.Characters.GetCurrentCharacter;

public static class GetCurrentCharacterEndpoint
{
  public static async Task<IResult> HandleAsync(
    ClaimsPrincipal user,
    GetCurrentCharacterHandler handler,
    CancellationToken cancellationToken
  )
  {
    var userIdClaim = user.FindFirstValue("sub");

    if (!Guid.TryParse(userIdClaim, out var userId))
      return ProblemDetailsExtensions.UnauthorizedProblem();

    var result = await handler.HandleAsync(
      new GetCurrentCharacterQuery(userId),
      cancellationToken
    );

    if (!result.Found)
    {
      return ProblemDetailsExtensions.NotFound(
        title: "Character not found.",
        detail: "The current user does not have a character.",
        code: GetCurrentCharacterHttpCodes.CharacterNotFound
      );
    }

    var response = new GetCurrentCharacterResponse(
      result.Id!.Value,
      result.Name!,
      ToApiFaction(result.Faction!.Value),
      result.Level!.Value,
      result.Experience!.Value,
      result.Gold!.Value,
      result.Energy!.Value,
      result.MaxEnergy!.Value,
      result.Strength!.Value,
      result.Defense!.Value,
      result.Agility!.Value,
      result.CreatedAtUtc!.Value
    );

    return Results.Ok(response);
  }

  private static string ToApiFaction(Faction faction)
  {
    return faction switch
    {
      Faction.Vampire => "vampire",
      Faction.Werewolf => "werewolf",
      _ => throw new ArgumentOutOfRangeException(nameof(faction), faction, "Unsupported faction")
    };
  }
}
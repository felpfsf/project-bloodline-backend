using System.Security.Claims;
using Bloodline.Api.Common.Http;
using Bloodline.Application.Characters.CreateCharacter;
using Bloodline.Domain.Characters;

namespace Bloodline.Api.Features.Characters.CreateCharacter;

public static class CreateCharacterEndpoint
{
  public static async Task<IResult> HandleAsync(
    CreateCharacterRequest request,
    ClaimsPrincipal user,
    CreateCharacterHandler handler,
    CancellationToken cancellationToken
  )
  {
    var userIdClaim = user.FindFirstValue("sub");

    if (!Guid.TryParse(userIdClaim, out var userId))
      return ProblemDetailsExtensions.UnauthorizedProblem();

    var faction = TryParseFaction(request.Faction, out var parsedFaction) ? parsedFaction : (Faction)0;
    var command = new CreateCharacterCommand(
      userId,
      request.Name,
      faction
    );

    var result = await handler.HandleAsync(command, cancellationToken);

    if (!result.IsSuccess)
      return ToErrorResult(result);

    var response = ToResponse(result);

    return Results.Created($"/api/characters/{response.Id}", response);
  }

  private static IResult ToErrorResult(CreateCharacterResult result)
  {
    return result.Error switch
    {
      CreateCharacterError.ValidationFailed => result.ValidationErrors.ValidationProblem(),
      CreateCharacterError.UserAlreadyHasCharacter => ProblemDetailsExtensions.ConflictProblem(
        title: "Character creation conflict",
        detail: "The user already has a character.",
        code: CreateCharacterHttpCodes.UserAlreadyHasCharacter
      ),
      CreateCharacterError.NameAlreadyTaken => ProblemDetailsExtensions.ConflictProblem(
        title: "Character creation conflict",
        detail: "The character name is already taken.",
        code: CreateCharacterHttpCodes.CharacterNameAlreadyTaken
      ),
      _ => ProblemDetailsExtensions.UnexpectedError()
    };
  }

  private static CreateCharacterResponse ToResponse(CreateCharacterResult result)
  {
    return new CreateCharacterResponse(
      result.Id!.Value,
      result.Name!,
      ToApiFactionName(result.Faction!.Value),
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
  }

  private static bool TryParseFaction(string faction, out Faction parsedFaction)
  {
    switch (faction.ToLowerInvariant())
    {
      case "vampire":
        parsedFaction = Faction.Vampire;
        return true;
      case "werewolf":
        parsedFaction = Faction.Werewolf;
        return true;
      default:
        parsedFaction = default;
        return false;
    }
  }

  private static string ToApiFactionName(Faction faction)
  {
    return faction switch
    {
      Faction.Vampire => "vampire",
      Faction.Werewolf => "werewolf",
      _ => throw new ArgumentOutOfRangeException(nameof(faction), $"{faction} is not a valid faction.")
    };
  }
}
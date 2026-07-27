using Bloodline.Api.Features.Characters.CreateCharacter;
using Bloodline.Api.Features.Characters.GetCurrentCharacter;

namespace Bloodline.Api.Features.Characters;

public static class CharacterEndpoints
{
  public static IEndpointRouteBuilder MapCharacterEndpoints(this IEndpointRouteBuilder endpoints)
  {
    var group = endpoints
        .MapGroup("/api/characters")
        .WithTags("Characters")
        .RequireAuthorization();

    group.MapPost(
      "/",
      CreateCharacterEndpoint.HandleAsync
    )
    .WithName("CreateCharacter")
    .Accepts<CreateCharacterRequest>("application/json")
    .Produces<CreateCharacterResponse>(StatusCodes.Status201Created)
    .ProducesProblem(StatusCodes.Status401Unauthorized)
    .ProducesProblem(StatusCodes.Status409Conflict);

    group.MapGet(
      "/me",
      GetCurrentCharacterEndpoint.HandleAsync
    )
    .WithName("GetCurrentCharacter")
    .Produces<GetCurrentCharacterResponse>(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status401Unauthorized)
    .ProducesProblem(StatusCodes.Status404NotFound);

    return endpoints;
  }
}

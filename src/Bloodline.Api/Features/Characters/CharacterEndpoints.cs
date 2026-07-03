using Bloodline.Api.Features.Characters.CreateCharacter;

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
    );

    return endpoints;
  }
}

using Bloodline.Api.Features.Hunts.GetActiveHunt;
using Bloodline.Api.Features.Hunts.StartHunt;

namespace Bloodline.Api.Features.Hunts;

public static class HuntEndpoints
{
  public static IEndpointRouteBuilder MapHuntEndpoints(
    this IEndpointRouteBuilder endpoints
  )
  {
    var group = endpoints
        .MapGroup("/api/hunts")
        .WithTags("Hunts")
        .RequireAuthorization();

    group.MapPost(
      "",
      StartHuntEndpoint.HandleAsync
    )
    .WithName("StartHunt")
    .Produces<StartHuntResponse>(StatusCodes.Status201Created)
    .ProducesProblem(StatusCodes.Status401Unauthorized)
    .ProducesProblem(StatusCodes.Status404NotFound)
    .ProducesProblem(StatusCodes.Status409Conflict);

    group.MapGet(
      "/active",
      GetActiveHuntEndpoint.HandleAsync
    )
    .WithName("GetActiveHunt")
    .Produces<GetActiveHuntResponse>(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status401Unauthorized)
    .ProducesProblem(StatusCodes.Status404NotFound);

    return endpoints;
  }
}
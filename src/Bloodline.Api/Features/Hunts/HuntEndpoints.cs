using Bloodline.Api.Features.Hunts.GetActiveHunt;
using Bloodline.Api.Features.Hunts.StartHunt;
using Bloodline.Api.Features.Hunts.CompleteHunt;

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

    group.MapPost(
      "/{id:guid}/complete",
      CompleteHuntEndpoint.HandleAsync
    )
    .WithName("CompleteHunt")
    .Produces<CompleteHuntResponse>(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status401Unauthorized)
    .ProducesProblem(StatusCodes.Status404NotFound)
    .ProducesProblem(StatusCodes.Status409Conflict);

    return endpoints;
  }
}
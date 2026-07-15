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
    );

    return endpoints;
  }
}
using Bloodline.Api.Features.Identity.Register;

namespace Bloodline.Api.Features.Identity;

public static class IdentityEndpoints
{
  public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
  {
    var group = endpoints.MapGroup("/api/auth").WithTags("Authentication");

    group.MapPost(
      "/register",
      RegisterUserEndpoint.HandleAsync
    );

    return endpoints;
  }
}
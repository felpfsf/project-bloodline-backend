using Bloodline.Api.Features.Identity.Login;
using Bloodline.Api.Features.Identity.Logout;
using Bloodline.Api.Features.Identity.Register;
using Bloodline.Api.Features.Me;

namespace Bloodline.Api.Features.Identity;

public static class IdentityEndpoints
{
  public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
  {
    var group = endpoints
        .MapGroup("/api/auth")
        .WithTags("Authentication");

    group.MapPost(
      "/register",
      RegisterUserEndpoint.HandleAsync
    );
    group.MapPost(
      "/login",
      LoginUserEndpoint.HandleAsync
    );

    group.MapGet(
      "/me",
      GetCurrentUserEndpoint.Handle
    ).RequireAuthorization();

    group.MapPost(
      "/logout",
      LogoutEndpoint.HandleAsync
    ).RequireAuthorization();

    return endpoints;
  }
}
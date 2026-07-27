using Bloodline.Api.Common.Http;
using Bloodline.Application.Identity.Login;

namespace Bloodline.Api.Features.Identity.Login;

public static class LoginUserEndpoint
{
  public static async Task<IResult> HandleAsync(
    LoginRequest request,
    LoginUserHandler handler,
    CancellationToken cancellationToken
  )
  {
    var command = new LoginCommand(
      request.Email,
      request.Password
    );

    var result = await handler.HandleAsync(command, cancellationToken);

    if (!result.IsSuccess)
    {
      return result.Error switch
      {
        LoginUserError.ValidationFailed => result.ValidationErrors.ValidationProblem(),
        LoginUserError.InvalidCredentials => ProblemDetailsExtensions.Problem(
          StatusCodes.Status401Unauthorized,
          "Invalid credentials",
          "The provided credentials are invalid. Please check your email and password and try again.",
          "invalid_credentials"
        ),
        _ => ProblemDetailsExtensions.UnexpectedError()
      };
    }

    var response = new LoginResponse(
      result.AccessToken!.Value,
      result.AccessToken!.ExpiresAtUtc
    );

    return Results.Ok(response);
  }
}
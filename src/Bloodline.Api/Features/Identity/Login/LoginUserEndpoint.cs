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
        LoginUserError.ValidationFailed =>
          Results.BadRequest(new
          {
            code = "validation_failed",
            message = "Validation failed.",
            errors = result.ValidationErrors.Select(error => new
            {
              property = error.PropertyName,
              code = error.ErrorCode,
              message = error.ErrorMessage
            })
          }),
        LoginUserError.InvalidCredentials => Results.Unauthorized(),
        _ => Results.Problem(
          title: "An unexpected error occurred.",
          statusCode: StatusCodes.Status500InternalServerError
          )
      };
    }

    var response = new LoginResponse(
      result.AccessToken!.Value,
      result.AccessToken!.ExpiresAtUtc
    );

    return Results.Ok(response);
  }
}
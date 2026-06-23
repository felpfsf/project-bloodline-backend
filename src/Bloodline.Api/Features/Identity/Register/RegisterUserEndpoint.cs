using Bloodline.Application.Identity.Register;

namespace Bloodline.Api.Features.Identity.Register;

public static class RegisterUserEndpoint
{
  public static async Task<IResult> HandleAsync(
    RegisterUserRequest request,
    RegisterUserHandler handler,
    CancellationToken cancellationToken
  )
  {
    var command = new RegisterUserCommand(
      request.Email,
      request.Password
    );

    var result = await handler.HandleAsync(command, cancellationToken);

    if (!result.IsSuccess)
    {
      return result.Error switch
      {
        RegisterUserError.ValidationFailed => Results.BadRequest(new
        {
          code = RegisterUserHttpCodes.ValidationFailed,
          message = "One or more validation errors occurred.",
          errors = result.ValidationErrors.Select(error => new
          {
            property = error.PropertyName,
            code = error.ErrorCode,
            message = error.ErrorMessage
          })
        }),

        RegisterUserError.EmailAlreadyExists => Results.Conflict(new
        {
          code = RegisterUserHttpCodes.EmailAlreadyExists,
          message = "A user with the provided email already exists."
        }),
        _ => Results.Problem(
          title: "An unexpected error occurred.",
          statusCode: StatusCodes.Status500InternalServerError
        )


      };
    }

    var response = new RegisterUserResponse(
      result.UserId!.Value,
      result.Email!,
      result.CreatedAtUtc!.Value
    );

    return Results.Created($"/api/users/{response.UserId}", response);
  }
}
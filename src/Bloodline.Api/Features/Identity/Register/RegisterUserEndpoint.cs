using Bloodline.Api.Common.Http;
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
        RegisterUserError.ValidationFailed => result.ValidationErrors.ValidationProblem(),
        RegisterUserError.EmailAlreadyExists => ProblemDetailsExtensions.ConflictProblem(
          title: "Registration conflict",
          detail: "A user with the provided email already exists.",
          code: RegisterUserHttpCodes.EmailAlreadyExists
        ),
        _ => ProblemDetailsExtensions.UnexpectedError()

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
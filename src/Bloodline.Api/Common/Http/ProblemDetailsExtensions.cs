using System.Diagnostics;
using Bloodline.Application.Common.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Bloodline.Api.Common.Http;

public static class ProblemDetailsExtensions
{
  public static IResult ValidationProblem(this IReadOnlyCollection<ValidationError> validationErrors)
  {
    var errors = validationErrors
      .GroupBy(error => error.PropertyName)
      .ToDictionary(
        group => group.Key,
        group => group
            .Select(error => error.ErrorMessage)
            .ToArray()
      );

    // var problemDetails = new ValidationProblemDetails(errors)
    // {
    //   Type = "https://httpstatuses.com/400", // https://bloodline.com/probs/validation
    //   Title = "Validation failed",
    //   Status = StatusCodes.Status400BadRequest
    // };

    // problemDetails.Extensions["traceId"] = Activity.Current?.Id;

    // return Results.ValidationProblem(
    //   errors,
    //   title: problemDetails.Title,
    //   type: problemDetails.Type,
    //   statusCode: problemDetails.Status
    // );

    return Results.ValidationProblem(
      errors,
      title: "Validation failed",
      type: "https://httpstatuses.com/400", // https://bloodline.com/probs/validation
      statusCode: StatusCodes.Status400BadRequest,
      extensions: new Dictionary<string, object?>
      {
        ["traceId"] = Activity.Current?.Id
      }
    );

  }

  public static IResult ConflictProblem(
    string title,
    string detail,
    string code
  )
  {
    return Problem(StatusCodes.Status409Conflict, title, detail, code);
  }

  public static IResult UnauthorizedProblem(string detail = "The provided authentication token is invalid.")
  {
    return Problem(
      StatusCodes.Status401Unauthorized,
      "Unauthorized",
      detail,
      "unauthorized"
    );
  }

  public static IResult NotFound(
    string title,
    string detail,
    string code
  )
  {
    return Problem(StatusCodes.Status404NotFound, title, detail, code);
  }

  public static IResult InternalServerError(
    string title,
    string detail,
    string code
  )
  {
    return Problem(StatusCodes.Status500InternalServerError, title, detail, code);
  }

  public static IResult UnexpectedError()
  {
    return InternalServerError(
      "Unexpected error",
      "An unexpected error occurred. Please try again later.",
      "internal_server_error"
    );
  }

  public static IResult Problem(
    int statusCode,
    string title,
    string detail,
    string code,
    string? traceId = null
  )
  {
    return Results.Problem(
      statusCode: statusCode,
      title: title,
      detail: detail,
      type: $"https://httpstatuses.com/{statusCode}",
      extensions: new Dictionary<string, object?>
      {
        ["code"] = code,
        ["traceId"] = traceId ?? Activity.Current?.Id
      }
    );
  }
}
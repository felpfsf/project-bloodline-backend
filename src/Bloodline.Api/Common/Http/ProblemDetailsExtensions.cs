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
    return Results.Problem(
      statusCode: StatusCodes.Status409Conflict,
      title: title,
      detail: detail,
      type: "https://httpstatuses.com/409", // https://bloodline.com/probs/conflict
      extensions: new Dictionary<string, object?>
      {
        ["code"] = code,
        ["traceId"] = Activity.Current?.Id
      }
    );
  }

  public static IResult UnauthorizedProblem()
  {
    return Results.Problem(
      statusCode: StatusCodes.Status401Unauthorized,
      title: "Invalid credentials",
      detail: "The provided credentials are invalid. Please check your email and password and try again.",
      type: "https://httpstatuses.com/401", // https://bloodline.com/probs/unauthorized
      extensions: new Dictionary<string, object?>
      {
        ["code"] = "invalid_credentials",
        ["traceId"] = Activity.Current?.Id
      }
    );
  }

  public static IResult NotFound(
    string title,
    string detail,
    string code
  )
  {
    return Results.Problem(
      statusCode: StatusCodes.Status404NotFound,
      title: title,
      detail: detail,
      type: "https://httpstatuses.com/404", // https://bloodline.com/probs/not_found
      extensions: new Dictionary<string, object?>
      {
        ["code"] = code,
        ["traceId"] = Activity.Current?.Id
      }
    );
  }

  public static IResult InternalServerError(
    string title,
    string detail,
    string code
  )
  {
    return Results.Problem(
      statusCode: StatusCodes.Status500InternalServerError,
      title: title,
      detail: detail,
      type: "https://httpstatuses.com/500", // https://bloodline.com/probs/internal_server_error
      extensions: new Dictionary<string, object?>
      {
        ["code"] = code,
        ["traceId"] = Activity.Current?.Id
      }
    );
  }
}
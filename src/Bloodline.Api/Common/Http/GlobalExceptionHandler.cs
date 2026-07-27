using System.Diagnostics;
using Bloodline.Application.Common.Persistence;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Bloodline.Api.Common.Http;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
  private readonly ILogger<GlobalExceptionHandler> _logger = logger;

  public async ValueTask<bool> TryHandleAsync(
    HttpContext httpContext,
    Exception exception,
    CancellationToken cancellationToken
  )
  {
    var (statusCode, title, detail, code) = exception switch
    {
      ConcurrencyConflictException => (
        StatusCodes.Status409Conflict,
        "Concurrent update",
        "The requested resource changed before this operation could be completed.",
        "concurrency_conflict"
      ),
      PersistenceException => (
        StatusCodes.Status500InternalServerError,
        "Persistence failure",
        "An unexpected error occurred. Please try again later.",
        "persistence_failure"
      ),
      _ => (
        StatusCodes.Status500InternalServerError,
        "Unexpected error",
        "An unexpected error occurred. Please try again later.",
        "internal_server_error"
      )
    };

    _logger.LogError(
      exception,
      "Unhandled request exception. StatusCode: {StatusCode}; ErrorCode: {ErrorCode}; TraceId: {TraceId}",
      statusCode,
      code,
      Activity.Current?.Id ?? httpContext.TraceIdentifier
    );

    await ProblemDetailsExtensions.Problem(
      statusCode,
      title,
      detail,
      code,
      Activity.Current?.Id ?? httpContext.TraceIdentifier
    ).ExecuteAsync(httpContext);

    return true;
  }
}
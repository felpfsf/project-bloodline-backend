namespace Bloodline.Application.Common.Validation;

public sealed record ValidationError(
  string PropertyName,
  string ErrorCode,
  string ErrorMessage
);
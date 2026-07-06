namespace Bloodline.Application.Common.Persistence;

public sealed class UniqueConstraintViolationException(
  string constraintName,
  Exception innerException
  ) : Exception($"Unique constraint '{constraintName}' was violated.", innerException)
{
  public string ConstraintName { get; } = constraintName;
}
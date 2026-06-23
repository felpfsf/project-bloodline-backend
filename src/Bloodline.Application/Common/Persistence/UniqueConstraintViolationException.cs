namespace Bloodline.Application.Common.Persistence;

public sealed class UniqueConstraintViolationException : Exception
{
  public UniqueConstraintViolationException(
    string constraintName,
    Exception innerException
  ) : base($"Unique constraint '{constraintName}' was violated.", innerException)
  {
    ConstraintName = constraintName;
  }

  public string ConstraintName { get; }
}
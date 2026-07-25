namespace Bloodline.Application.Common.Persistence;

public sealed class ConcurrencyConflictException(string message, Exception? innerException = null) : Exception(message, innerException)
{
}
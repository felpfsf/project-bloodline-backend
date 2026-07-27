namespace Bloodline.Application.Common.Persistence;

public sealed class PersistenceException(string message, Exception innerException) : Exception(message, innerException)
{
}
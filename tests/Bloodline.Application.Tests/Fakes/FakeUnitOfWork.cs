using Bloodline.Application.Common.Persistence;

namespace Bloodline.Application.Tests.Fakes;

public sealed class FakeUnitOfWork : IUnitOfWork
{
  public int SaveChangesCallCount { get; private set; }
  public UniqueConstraintViolationException? UniqueConstraintToThrow { get; set; }
  public ConcurrencyConflictException? ConcurrencyConflictToThrow { get; set; }
  public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
  {
    SaveChangesCallCount++;

    if (UniqueConstraintToThrow is not null)
    {
      throw UniqueConstraintToThrow;
    }

    if (ConcurrencyConflictToThrow is not null)
    {
      throw ConcurrencyConflictToThrow;
    }

    return Task.FromResult(1);
  }
}
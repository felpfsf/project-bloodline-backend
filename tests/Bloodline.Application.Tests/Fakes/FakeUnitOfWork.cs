using Bloodline.Application.Common.Persistence;

namespace Bloodline.Application.Tests.Fakes;

public sealed class FakeUnitOfWork : IUnitOfWork
{
  public int SaveChangesCallCount { get; private set; }
  public Task SaveChangesAsync(CancellationToken cancellationToken)
  {
    SaveChangesCallCount++;
    return Task.CompletedTask;
  }
}
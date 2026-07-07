namespace Bloodline.Application.Common.Persistence;

public interface IUnitOfWork
{
  Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
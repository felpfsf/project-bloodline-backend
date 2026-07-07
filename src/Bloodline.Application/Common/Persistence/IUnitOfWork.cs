namespace Bloodline.Application.Common.Persistence;

public interface IUnitOfWork
{
  Task SaveChangesAsync(CancellationToken cancellationToken);
}
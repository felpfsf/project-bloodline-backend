using Bloodline.Application.Common.Time;

namespace Bloodline.Infrastructure.Common.Time;

public sealed class SystemClock : IClock
{
  public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
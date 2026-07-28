using Bloodline.Application.Common.Time;

namespace Bloodline.IntegrationTests.Infrastructure;

public sealed class IntegrationTestClock : IClock
{
  public DateTimeOffset UtcNow { get; set; }
}
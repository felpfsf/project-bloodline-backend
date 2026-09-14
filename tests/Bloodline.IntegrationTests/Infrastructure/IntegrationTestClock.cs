using Bloodline.Application.Common.Time;

namespace Bloodline.IntegrationTests.Infrastructure;

public sealed class IntegrationTestClock : IClock
{
  public DateTimeOffset UtcNow { get; set; } = new(
    2026,
    7,
    1,
    0,
    0,
    0,
    TimeSpan.Zero
  );
}
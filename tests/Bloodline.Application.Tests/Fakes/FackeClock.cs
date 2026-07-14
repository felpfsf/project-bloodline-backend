using Bloodline.Application.Common.Time;

namespace Bloodline.Application.Tests.Fakes;

public sealed class FakeClock : IClock
{

  public DateTimeOffset UtcNow { get; set; }
}
using Bloodline.Application.Common.Time;

namespace Bloodline.Application.Tests.Identity.Register.Fakes;

internal sealed class FakeClock : IClock
{

  public DateTimeOffset UtcNow { get; set; }
}
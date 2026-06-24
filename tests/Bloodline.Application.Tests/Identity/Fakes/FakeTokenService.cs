using Bloodline.Application.Identity.Abstractions;
using Bloodline.Application.Identity.Models;

namespace Bloodline.Application.Tests.Identity.Fakes;

internal sealed class FakeTokenService : ITokenService
{
  public AccessToken TokenToReturn { get; set; } = new(
    "fake-access-token",
    new DateTimeOffset(2026, 6, 23, 20, 0, 0, TimeSpan.Zero)
  );

  public Guid? ReceivedUserId { get; private set; }

  public AccessToken Generate(Guid userId)
  {
    ReceivedUserId = userId;

    return TokenToReturn;
  }
}
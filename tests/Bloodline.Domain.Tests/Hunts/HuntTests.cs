using Bloodline.Domain.Hunts;
using FluentAssertions;

namespace Bloodline.Domain.Tests.Hunts;

public sealed class HuntTests
{
  [Fact]
  public void Start_ShouldCreateActiveHunt_WhenParametersAreValid()
  {
    var startsAtUtc = new DateTimeOffset(2026, 7, 6, 12, 0, 0, TimeSpan.Zero);

    var hunt = Hunt.Start(
      Guid.NewGuid(),
      "shadow_hunt",
      startsAtUtc,
      TimeSpan.FromMinutes(30),
      10,
      50,
      75
    );

    hunt.Status.Should().Be(HuntStatus.Active);
    hunt.StartsAtUtc.Should().Be(startsAtUtc);
    hunt.EndsAtUtc.Should().Be(startsAtUtc.AddMinutes(30));
    hunt.CompletedAtUtc.Should().BeNull();
    hunt.EnergyCost.Should().Be(10);
    hunt.ExperienceReward.Should().Be(50);
    hunt.GoldReward.Should().Be(75);
  }

  [Fact]
  public void CanCompleteAt_ShouldReturnFalse_WhenHuntIsStillRunning()
  {
    var startsAtUtc = new DateTimeOffset(2026, 7, 6, 12, 0, 0, TimeSpan.Zero);
    var hunt = CreateHunt(startsAtUtc, TimeSpan.FromMinutes(30));

    var canComplete = hunt.CanCompleteAt(startsAtUtc.AddMinutes(29));

    canComplete.Should().BeFalse();
  }

  [Fact]
  public void Complete_ShouldMarkHuntAsCompleted_WhenHuntHasEnded()
  {
    var startsAtUtc = new DateTimeOffset(2026, 7, 6, 12, 0, 0, TimeSpan.Zero);
    var completedAtUtc = startsAtUtc.AddMinutes(30);
    var hunt = CreateHunt(startsAtUtc, TimeSpan.FromMinutes(30));

    hunt.Complete(completedAtUtc);

    hunt.Status.Should().Be(HuntStatus.Completed);
    hunt.CompletedAtUtc.Should().Be(completedAtUtc);
  }

  [Fact]
  public void Complete_ShouldThrow_WhenHuntHasNotEndedYet()
  {
    var startsAtUtc = new DateTimeOffset(2026, 7, 6, 12, 0, 0, TimeSpan.Zero);
    var hunt = CreateHunt(startsAtUtc, TimeSpan.FromMinutes(30));

    var action = () => hunt.Complete(startsAtUtc.AddMinutes(29));

    action.Should().Throw<InvalidOperationException>()
      .WithMessage("Cannot complete hunt before it ends.");
  }

  [Fact]
  public void Complete_ShouldThrow_WhenHuntIsAlreadyCompleted()
  {
    var startsAtUtc = new DateTimeOffset(2026, 7, 6, 12, 0, 0, TimeSpan.Zero);
    var completedAtUtc = startsAtUtc.AddMinutes(30);
    var hunt = CreateHunt(startsAtUtc, TimeSpan.FromMinutes(30));
    hunt.Complete(completedAtUtc);

    var action = () => hunt.Complete(completedAtUtc.AddMinutes(1));

    action.Should().Throw<InvalidOperationException>()
      .WithMessage("Only active hunts can be completed.");
  }

  private static Hunt CreateHunt(DateTimeOffset startsAtUtc, TimeSpan duration)
  {
    return Hunt.Start(
      Guid.NewGuid(),
      "shadow_hunt",
      startsAtUtc,
      duration,
      10,
      50,
      75
    );
  }

  [Fact]
  public void Start_ShouldThrow_WhenDefinitionCodeIsNullOrEmpty()
  {
    var startsAtUtc = new DateTimeOffset(2026, 7, 6, 12, 0, 0, TimeSpan.Zero);

    var action = () => Hunt.Start(
      Guid.NewGuid(),
      " ",
      startsAtUtc,
      TimeSpan.FromSeconds(60),
      10,
      25,
      15
    );

    var exception = action.Should().Throw<ArgumentException>().Which;

    exception.Message.Should().StartWith("Hunt definition code is required.");
    exception.ParamName.Should().Be("definitionCode");
  }
}
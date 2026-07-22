namespace Bloodline.Application.Hunts.CompleteHunt;

public sealed record CompleteHuntCommand(
  Guid UserId,
  Guid HuntId
);
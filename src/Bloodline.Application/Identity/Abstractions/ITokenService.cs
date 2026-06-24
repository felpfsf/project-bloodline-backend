using Bloodline.Application.Identity.Models;

namespace Bloodline.Application.Identity.Abstractions;

public interface ITokenService
{
  AccessToken Generate(
    Guid userId
  );
}

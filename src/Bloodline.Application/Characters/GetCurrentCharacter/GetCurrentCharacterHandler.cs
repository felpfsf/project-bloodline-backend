using Bloodline.Application.Characters.Abstractions;

namespace Bloodline.Application.Characters.GetCurrentCharacter;

public sealed class GetCurrentCharacterHandler(ICharacterRepository characterRepository)
{
  private readonly ICharacterRepository _characterRepository = characterRepository;

  public async Task<GetCurrentCharacterResult> HandleAsync(
    GetCurrentCharacterQuery query,
    CancellationToken cancellationToken
  )
  {
    var character = await _characterRepository.GetByUserIdAsync(query.UserId, cancellationToken);

    if (character is null) return GetCurrentCharacterResult.NotFound();


    return GetCurrentCharacterResult.Success(character);
  }
}
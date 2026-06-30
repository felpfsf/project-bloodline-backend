using Bloodline.Application.Characters.Abstractions;
using Bloodline.Application.Common.Persistence;
using Bloodline.Application.Common.Time;
using Bloodline.Application.Common.Validation;
using Bloodline.Domain.Characters;
using FluentValidation;

namespace Bloodline.Application.Characters.CreateCharacter;

public sealed class CreateCharacterHandler
{
  private readonly ICharacterRepository _characterRepository;
  private readonly IClock _clock;
  private readonly IValidator<CreateCharacterCommand> _validator;

  public CreateCharacterHandler(
    ICharacterRepository characterRepository,
    IClock clock,
    IValidator<CreateCharacterCommand> validator
  )
  {
    _characterRepository = characterRepository;
    _clock = clock;
    _validator = validator;
  }

  public async Task<CreateCharacterResult> HandleAsync(
    CreateCharacterCommand command,
    CancellationToken cancellationToken
  )
  {
    var validationResult = await _validator.ValidateAsync(command, cancellationToken);

    if (!validationResult.IsValid)
    {
      var errors = validationResult.Errors
          .Select(error => new ValidationError(
            error.PropertyName,
            error.ErrorCode,
            error.ErrorMessage
          ))
          .ToList();

      return CreateCharacterResult.ValidationFailure(errors);
    }

    var normalizedName = command.Name.Trim().ToUpperInvariant();

    var userAlreadyHasCharacter = await _characterRepository.ExistsByUserIdAsync(command.UserId, cancellationToken);

    if (userAlreadyHasCharacter)
    {
      return CreateCharacterResult.Failure(CreateCharacterError.UserAlreadyHasCharacter);
    }

    var nameAlreadyTaken = await _characterRepository.ExistsByNormalizedNameAsync(normalizedName, cancellationToken);

    if (nameAlreadyTaken)
    {
      return CreateCharacterResult.Failure(CreateCharacterError.NameAlreadyTaken);
    }

    var character = Character.Create(
      command.UserId,
      command.Name.Trim(),
      command.Faction,
      _clock.UtcNow
    );

    try
    {
      await _characterRepository.CreateAsync(character, cancellationToken);
    }
    catch (UniqueConstraintViolationException exception)
      when (exception.ConstraintName == "ux_characters_user_id")
    {
      return CreateCharacterResult.Failure(CreateCharacterError.UserAlreadyHasCharacter);
    }
    catch (UniqueConstraintViolationException exception)
      when (exception.ConstraintName == "ux_characters_normalized_name")
    {
      return CreateCharacterResult.Failure(CreateCharacterError.NameAlreadyTaken);
    }

    return CreateCharacterResult.Success(
      id: character.Id,
      name: character.Name,
      faction: character.Faction,
      level: character.Level,
      experience: character.Experience,
      gold: character.Gold,
      energy: character.Energy,
      maxEnergy: character.MaxEnergy,
      strength: character.Strength,
      defense: character.Defense,
      agility: character.Agility,
      createdAtUtc: character.CreatedAtUtc
    );
  }

}
using Bloodline.Application.Characters.Abstractions;
using Bloodline.Application.Common.Persistence;
using Bloodline.Application.Common.Time;
using Bloodline.Application.Common.Validation;
using Bloodline.Domain.Characters;
using FluentValidation;

namespace Bloodline.Application.Characters.CreateCharacter;

public sealed class CreateCharacterHandler(
  ICharacterRepository characterRepository,
  IClock clock,
  IValidator<CreateCharacterCommand> validator
  )
{
  private readonly ICharacterRepository _characterRepository = characterRepository;
  private readonly IClock _clock = clock;
  private readonly IValidator<CreateCharacterCommand> _validator = validator;

  public async Task<CreateCharacterResult> HandleAsync(
    CreateCharacterCommand command,
    CancellationToken cancellationToken
  )
  {
    var validationGuard = await ValidateCommandAsync(command, cancellationToken);
    if (validationGuard is not null) return validationGuard;

    var normalizedName = command.Name.Trim().ToUpperInvariant();

    var userGuard = await CheckUserAlreadyHasCharacterAsync(command.UserId, cancellationToken);
    if (userGuard is not null) return userGuard;

    var nameGuard = await CheckNameAlreadyTakenAsync(normalizedName, cancellationToken);
    if (nameGuard is not null) return nameGuard;

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

  private async Task<CreateCharacterResult?> ValidateCommandAsync(CreateCharacterCommand command, CancellationToken cancellationToken)
  {
    var validationResult = await _validator.ValidateAsync(command, cancellationToken);

    if (validationResult.IsValid) return null;

    var errors = validationResult.Errors
        .Select(error => new ValidationError(
          error.PropertyName,
          error.ErrorCode,
          error.ErrorMessage
        ))
        .ToList();

    return CreateCharacterResult.ValidationFailure(errors);
  }

  private async Task<CreateCharacterResult?> CheckUserAlreadyHasCharacterAsync(Guid userId, CancellationToken cancellationToken)
  {
    var exists = await _characterRepository.ExistsByUserIdAsync(userId, cancellationToken);

    return exists
      ? CreateCharacterResult.Failure(CreateCharacterError.UserAlreadyHasCharacter)
      : null;
  }

  private async Task<CreateCharacterResult?> CheckNameAlreadyTakenAsync(string normalizedName, CancellationToken cancellationToken)
  {
    var taken = await _characterRepository.ExistsByNormalizedNameAsync(normalizedName, cancellationToken);

    return taken
      ? CreateCharacterResult.Failure(CreateCharacterError.NameAlreadyTaken)
      : null;
  }

}
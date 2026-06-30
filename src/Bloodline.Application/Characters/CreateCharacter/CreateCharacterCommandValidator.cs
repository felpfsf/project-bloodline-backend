using FluentValidation;

namespace Bloodline.Application.Characters.CreateCharacter;

public sealed class CreateCharacterCommandValidator : AbstractValidator<CreateCharacterCommand>
{
  public CreateCharacterCommandValidator()
  {
    RuleFor(character => character.UserId)
      .NotEmpty();

    RuleFor(character => character.Name)
      .NotEmpty()
      .WithErrorCode(CreateCharacterErrorCodes.NameRequired)
      .MinimumLength(3)
      .WithErrorCode(CreateCharacterErrorCodes.NameMinLength)
      .MaximumLength(20)
      .WithErrorCode(CreateCharacterErrorCodes.NameMaxLength)
      .Matches("^[a-zA-Z0-9_-]+(?: [a-zA-Z0-9_-]+)*$")
      .WithErrorCode(CreateCharacterErrorCodes.NameInvalidFormat)
      .WithMessage("Name can only contain letters, numbers, spaces, underscores and hyphens, and cannot start or end with a space.");

    RuleFor(character => character.Faction)
      .IsInEnum()
      .WithErrorCode(CreateCharacterErrorCodes.FactionRequired);
  }
}
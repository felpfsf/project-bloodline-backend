namespace Bloodline.Application.Characters.CreateCharacter;

public static class CreateCharacterErrorCodes
{
  public const string NameRequired = "character_name_required";
  public const string NameMinLength = "character_name_min_length";
  public const string NameMaxLength = "character_name_max_length";
  public const string NameInvalidFormat = "character_name_invalid_format";

  public const string FactionRequired = "character_faction_required";
}
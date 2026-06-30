using Bloodline.Domain.Character;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloodline.Infrastructure.Persistence.Configurations;

public sealed class CharacterConfiguration : IEntityTypeConfiguration<Character>
{
  public void Configure(EntityTypeBuilder<Character> builder)
  {
    builder.ToTable("characters");

    builder.HasKey(character => character.Id);

    builder.Property(character => character.Id).HasColumnName("id");

    builder.Property(character => character.UserId)
        .HasColumnName("user_id")
        .IsRequired();

    builder.Property(character => character.Name)
        .HasColumnName("name")
        .HasMaxLength(20)
        .IsRequired();

    builder.Property(character => character.NormalizedName)
        .HasColumnName("normalized_name")
        .HasMaxLength(20)
        .IsRequired();

    builder.Property(character => character.Faction)
        .HasColumnName("faction")
        .HasConversion<short>()
        .IsRequired();

    builder.Property(character => character.Level)
        .HasColumnName("level")
        .IsRequired();

    builder.Property(character => character.Experience)
        .HasColumnName("experience")
        .IsRequired();

    builder.Property(character => character.Gold)
        .HasColumnName("gold")
        .IsRequired();

    builder.Property(character => character.Energy)
        .HasColumnName("energy")
        .IsRequired();

    builder.Property(character => character.MaxEnergy)
        .HasColumnName("max_energy")
        .IsRequired();

    builder.Property(character => character.Strength)
        .HasColumnName("strength")
        .IsRequired();

    builder.Property(character => character.Defense)
        .HasColumnName("defense")
        .IsRequired();

    builder.Property(character => character.Agility)
        .HasColumnName("agility")
        .IsRequired();

    builder.Property(character => character.CreatedAtUtc)
        .HasColumnName("created_at_utc")
        .IsRequired();

    builder.HasIndex(character => character.UserId)
        .IsUnique()
        .HasDatabaseName("ux_characters_user_id");

    builder.HasIndex(character => character.NormalizedName)
        .IsUnique()
        .HasDatabaseName("ux_characters_normalized_name");
  }
}
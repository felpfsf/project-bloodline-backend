using Bloodline.Domain.Characters;
using Bloodline.Domain.Hunts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloodline.Infrastructure.Hunts.Persistence;

public sealed class HuntConfiguration : IEntityTypeConfiguration<Hunt>
{
  public void Configure(EntityTypeBuilder<Hunt> builder)
  {
    builder.ToTable("hunts");

    builder.HasKey(hunt => hunt.Id);

    builder.Property(hunt => hunt.Id)
      .HasColumnName("id");

    builder.Property(hunt => hunt.CharacterId)
      .HasColumnName("character_id")
      .IsRequired();

    builder.Property(hunt => hunt.DefinitionCode)
      .HasColumnName("definition_code")
      .HasMaxLength(64)
      .IsRequired();

    builder.Property(hunt => hunt.Status)
      .HasColumnName("status")
      .HasConversion<int>()
      .IsRequired();

    builder.Property(hunt => hunt.StartsAtUtc)
      .HasColumnName("starts_at_utc")
      .IsRequired();

    builder.Property(hunt => hunt.EndsAtUtc)
      .HasColumnName("ends_at_utc")
      .IsRequired();

    builder.Property(hunt => hunt.CompletedAtUtc)
      .HasColumnName("completed_at_utc");

    builder.Property(hunt => hunt.EnergyCost)
      .HasColumnName("energy_cost")
      .IsRequired();

    builder.Property(hunt => hunt.ExperienceReward)
      .HasColumnName("experience_reward")
      .IsRequired();

    builder.Property(hunt => hunt.GoldReward)
      .HasColumnName("gold_reward")
      .IsRequired();

    builder.Property(hunt => hunt.Version)
      .HasColumnName("version")
      .IsRequired()
      .IsConcurrencyToken();

    builder.HasOne<Character>()
      .WithMany()
      .HasForeignKey(hunt => hunt.CharacterId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.HasIndex(hunt => new { hunt.CharacterId, hunt.Status })
      .HasDatabaseName("ix_hunts_character_id_status");

    builder.HasIndex(hunt => hunt.CharacterId)
      .IsUnique()
      .HasDatabaseName("ux_hunts_character_id_active")
      .HasFilter("\"status\" = 1");
  }
}
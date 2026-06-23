using Bloodline.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bloodline.Infrastructure.Identity.Persistence;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    builder.ToTable("users");

    builder.HasKey(user => user.Id);

    builder.Property(user => user.Email)
        .HasColumnName("email")
        .HasMaxLength(320)
        .IsRequired();

    builder.Property(user => user.NormalizedEmail)
        .HasColumnName("normalized_email")
        .HasMaxLength(320)
        .IsRequired();

    builder.Property(user => user.PasswordHash)
        .HasColumnName("password_hash")
        .HasMaxLength(512)
        .IsRequired();

    builder.Property(user => user.CreatedAtUtc)
        .HasColumnName("created_at_utc")
        .IsRequired();

    builder.HasIndex(user => user.NormalizedEmail)
        .IsUnique()
        .HasDatabaseName("ux_users_normalized_email");
  }
}
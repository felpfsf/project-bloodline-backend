namespace Bloodline.Domain.Identity;

public sealed class User
{
  private User()
  {
    Email = null!;
    NormalizedEmail = null!;
    PasswordHash = null!;
  }
  private User(
    Guid id,
    string email,
    string normalizedEmail,
    string passwordHash,
    DateTimeOffset createdAtUtc
  )
  {
    Id = id;
    Email = email;
    NormalizedEmail = normalizedEmail;
    PasswordHash = passwordHash;
    CreatedAtUtc = createdAtUtc;

  }

  public Guid Id { get; private set; }
  public string Email { get; private set; }
  public string NormalizedEmail { get; private set; }
  public string PasswordHash { get; private set; }
  public DateTimeOffset CreatedAtUtc { get; private set; }

  public static User Create(
    Guid id,
    string email,
    string normalizedEmail,
    string passwordHash,
    DateTimeOffset createdAtUtc
  )
  {
    return new User(
      id,
      email,
      normalizedEmail,
      passwordHash,
      createdAtUtc
    );
  }
}
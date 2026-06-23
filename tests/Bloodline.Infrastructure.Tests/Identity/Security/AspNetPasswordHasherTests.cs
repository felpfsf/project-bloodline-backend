using Bloodline.Infrastructure.Identity.Security;

namespace Bloodline.Infrastructure.Tests.Identity.Security;

public sealed class AspNetPasswordHasherTests
{
  [Fact]
  public void Hash_ShouldNotReturnPlainTextPassword()
  {
    // Arrange
    var passwordHasher = new AspNetPasswordHasher();
    var password = "my-secure-password";

    // Act
    var hashedPassword = passwordHasher.HashPassword(password);

    // Assert
    Assert.NotEqual(password, hashedPassword);
  }

  [Fact]
  public void Verify_ShouldReturnTrueForValidPassword()
  {
    // Arrange
    var passwordHasher = new AspNetPasswordHasher();
    var password = "my-secure-password";
    var hashedPassword = passwordHasher.HashPassword(password);

    // Act
    var isValid = passwordHasher.VerifyHashedPassword(hashedPassword, password);

    // Assert
    Assert.True(isValid);
  }
  
  [Fact]
  public void Verify_ShouldReturnFalseForInvalidPassword()
  {
    // Arrange
    var passwordHasher = new AspNetPasswordHasher();
    var password = "my-secure-password";
    var hashedPassword = passwordHasher.HashPassword(password);

    // Act
    var isValid = passwordHasher.VerifyHashedPassword(hashedPassword, "invalid-password");

    // Assert
    Assert.False(isValid);
  }

  [Fact]
  public void HashPassword_ShouldReturnHashedPassword()
  {
    // Arrange
    var passwordHasher = new AspNetPasswordHasher();
    var password = "my-secure-password";

    // Act
    var hashedPassword = passwordHasher.HashPassword(password);

    // Assert
    Assert.NotNull(hashedPassword);
    Assert.NotEmpty(hashedPassword);
  }
}
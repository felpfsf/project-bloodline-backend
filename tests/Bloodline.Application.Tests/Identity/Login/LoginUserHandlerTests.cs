using Bloodline.Application.Identity.Login;
using Bloodline.Application.Tests.Identity.Fakes;
using Bloodline.Domain.Identity;

namespace Bloodline.Application.Tests.Identity.Login;

public sealed class LoginUserHandlerTests
{
  private const string fakeEmail = "test@example.com";
  private const string fakePassword = "password123";
  private const string fakeNormalizedEmail = "TEST@EXAMPLE.COM";
  private const string fakeHashedPassword = "hashed-password";

  private static LoginUserHandler LoginHandler(
    FakeUserRepository repository,
    FakePasswordHasher passwordHasher
  )
  {
    return new LoginUserHandler(
      repository,
      passwordHasher,
      new LoginUserCommandValidator()
    );
  }

  [Fact]
  public async Task HandleAsync_ShouldReturnSuccess_WhenCredentialsAreValid()
  {
    // Arrange
    var user = User.Create(
          Guid.NewGuid(),
          fakeEmail,
          fakeNormalizedEmail,
          fakeHashedPassword,
          DateTimeOffset.UtcNow
        );
    var repository = new FakeUserRepository { UserToReturn = user };
    var passwordHasher = new FakePasswordHasher { VerificationResult = true };
    var handler = LoginHandler(repository, passwordHasher);
    var command = new LoginCommand($" {fakeEmail} ", fakePassword);

    // Act
    var result = await handler.HandleAsync(command, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(user.Id, result.UserId);
    Assert.Equal(user.Email, result.Email);

    Assert.Equal(fakeNormalizedEmail, repository.RequestedNormalizedEmail);
    Assert.Equal(fakeHashedPassword, passwordHasher.ReceivedHashedPassword);
    Assert.Equal(fakePassword, passwordHasher.ReceivedProvidedPassword);


  }

  [Fact]
  public async Task HandleAsync_ShouldReturnFailure_WhenUserDoesNotExist()
  {
    // Arrange
    var repository = new FakeUserRepository { UserToReturn = null };
    var passwordHasher = new FakePasswordHasher();
    var handler = LoginHandler(repository, passwordHasher);
    var command = new LoginCommand(fakeEmail, fakePassword);

    // Act
    var result = await handler.HandleAsync(command, CancellationToken.None);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(LoginUserError.InvalidCredentials, result.Error);
    Assert.Null(passwordHasher.ReceivedProvidedPassword);
  }

  [Fact]
  public async Task HandleAsync_ShouldReturnFailure_WhenPasswordIsIncorrect()
  {
    // Arrange
    var user = User.Create(
          Guid.NewGuid(),
          fakeEmail,
          fakeNormalizedEmail,
          fakeHashedPassword,
          DateTimeOffset.UtcNow
        );
    var repository = new FakeUserRepository { UserToReturn = user };
    var passwordHasher = new FakePasswordHasher { VerificationResult = false };
    var handler = LoginHandler(repository, passwordHasher);
    var command = new LoginCommand(fakeEmail, "WrongPassword");

    // Act
    var result = await handler.HandleAsync(command, CancellationToken.None);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(LoginUserError.InvalidCredentials, result.Error);
  }

  [Fact]
  public async Task HandleAsync_ShouldReturnFailure_WhenCommandIsInvalid()
  {
    // Arrange
    var repository = new FakeUserRepository();
    var passwordHasher = new FakePasswordHasher();
    var handler = LoginHandler(repository, passwordHasher);
    var command = new LoginCommand(string.Empty, string.Empty);

    // Act
    var result = await handler.HandleAsync(command, CancellationToken.None);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(LoginUserError.ValidationFailed, result.Error);
    Assert.NotEmpty(result.ValidationErrors);
    Assert.Null(repository.RequestedNormalizedEmail);
    Assert.Null(passwordHasher.ReceivedProvidedPassword);
  }
}
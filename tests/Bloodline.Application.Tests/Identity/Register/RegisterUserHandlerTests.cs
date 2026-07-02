using Bloodline.Application.Identity.Register;
using Bloodline.Application.Tests.Fakes;
using Bloodline.Application.Tests.Identity.Fakes;

namespace Bloodline.Application.Tests.Identity.Register;

public sealed class RegisterUserHandlerTests
{
  private const string fakeEmail = "test@example.com";
  private const string fakePassword = "password123";
  private const string hashedPassword = "generated-hash";

  private static RegisterUserHandler CreateHandler(
    FakeUserRepository repository,
    FakePasswordHasher passwordHasher,
    FakeClock clock
  )
  {
    return new RegisterUserHandler(
      repository,
      passwordHasher,
      clock,
      new RegisterUserCommandValidator()
    );
  }

  [Fact]
  public async Task HandleAsync_ShouldCreateUser_WithEmailIsAvailable()
  {
    // Arrange
    var repository = new FakeUserRepository { UserExists = false };
    var passwordHasher = new FakePasswordHasher { HashToReturn = hashedPassword };
    var expectedCreatedAt = new DateTimeOffset(
      2026, 6, 17, 12, 0, 0, TimeSpan.Zero
    );
    var clock = new FakeClock { UtcNow = expectedCreatedAt };
    var handler = CreateHandler(repository, passwordHasher, clock);
    var command = new RegisterUserCommand(fakeEmail, fakePassword);

    // Act
    var result = await handler.HandleAsync(command, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(repository.CreatedUser);
    Assert.Equal(fakeEmail, repository.CreatedUser.Email);
    Assert.Equal(hashedPassword, repository.CreatedUser.PasswordHash);
    Assert.Equal(fakePassword, passwordHasher.ReceivedPassword);
    Assert.Equal(expectedCreatedAt, repository.CreatedUser.CreatedAtUtc);
    Assert.Equal(repository.CreatedUser.Id, result.UserId);
    Assert.Equal(repository.CreatedUser.Email, result.Email);
    Assert.Equal(repository.CreatedUser.CreatedAtUtc, result.CreatedAtUtc);
  }

  [Fact]
  public async Task HandleAsync_ShouldReturnFailure_WhenEmailAlreadyExists()
  {
    // Arrange
    var repository = new FakeUserRepository
    {
      UserExists = true
    };

    var passwordHasher = new FakePasswordHasher();

    var clock = new FakeClock
    {
      UtcNow = DateTimeOffset.UtcNow
    };

    var handler = CreateHandler(repository, passwordHasher, clock);

    var command = new RegisterUserCommand(
        fakeEmail,
        fakePassword);

    // Act
    var result = await handler.HandleAsync(
        command,
        CancellationToken.None);

    // Assert
    Assert.False(result.IsSuccess);

    Assert.Equal(
        RegisterUserError.EmailAlreadyExists,
        result.Error);

    Assert.Null(repository.CreatedUser);
    Assert.Null(passwordHasher.ReceivedPassword);
  }

  [Fact]
  public async Task HandleAsync_ShouldNormalizeEmail_BeforeCheckingDuplication()
  {
    // Arrange
    var repository = new FakeUserRepository
    {
      UserExists = true
    };

    var handler = CreateHandler(repository, new FakePasswordHasher(), new FakeClock());

    var command = new RegisterUserCommand(
        $"  {fakeEmail}  ",
        fakePassword);

    // Act
    await handler.HandleAsync(
        command,
        CancellationToken.None);

    // Assert
    Assert.Equal(
        fakeEmail.ToUpper(),
        repository.CheckedNormalizedEmail);
  }

  [Fact]
  public async Task HandleAsync_ShouldReturnValidationFailure_WhenCommandIsInvalid()
  {
    // Arrange
    var repository = new FakeUserRepository();
    var passwordHasher = new FakePasswordHasher();
    var clock = new FakeClock();
    var handler = CreateHandler(repository, passwordHasher, clock);
    var command = new RegisterUserCommand("", "");

    // Act
    var result = await handler.HandleAsync(command, CancellationToken.None);

    // Assert
    Assert.False(result.IsSuccess);

    Assert.Equal(RegisterUserError.ValidationFailed, result.Error);

    Assert.NotEmpty(result.ValidationErrors!);

    Assert.Contains(
      result.ValidationErrors,
      error => error.ErrorCode == "email_required"
    );

    Assert.Contains(
      result.ValidationErrors,
      error => error.ErrorCode == "password_required"
    );

    Assert.Null(repository.CreatedUser);
    Assert.Null(passwordHasher.ReceivedPassword);
  }

  [Fact]
  public async Task HandleAsync_ShouldReturnEmailAlreadyExists_WhenUniqueConstraintViolationOccurs()
  {
    // Arrange
    var repository = new FakeUserRepository
    {
      UserExists = false,
      ThrowUniqueConstraintViolationException = true
    };

    var passwordHasher = new FakePasswordHasher();

    var clock = new FakeClock
    {
      UtcNow = new DateTimeOffset(2026, 6, 22, 21, 0, 0, TimeSpan.Zero)
    };

    var handler = CreateHandler(repository, passwordHasher, clock);

    var command = new RegisterUserCommand(fakeEmail, fakePassword);

    // Act
    var result = await handler.HandleAsync(command, CancellationToken.None);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(RegisterUserError.EmailAlreadyExists, result.Error);


  }
}
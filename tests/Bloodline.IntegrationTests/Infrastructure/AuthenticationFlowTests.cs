using System.Net;
using System.Net.Http.Json;
using Bloodline.Api.Features.Identity.Login;
using Bloodline.Api.Features.Identity.Register;
using Bloodline.Api.Features.Me;

namespace Bloodline.IntegrationTests.Infrastructure;

[Collection(PostgreSqlCollection.Name)]
public sealed class AuthenticationFlowTests(PostgreSqlFixture fixture)
{
  private readonly BloodlineWebApplicationFactory _factory = fixture.Factory;

  [Fact]
  public async Task RegisterLoginAndMe_ShouldAuthenticateUser()
  {

    // Arrange
    using var client = _factory.CreateClient();

    var email = $"player{Guid.NewGuid():N}@bloodline.com";
    const string password = "TestPassword123!";

    // Act - Register
    var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new
    {
      email,
      password
    });
    var registerBody = await registerResponse.Content.ReadFromJsonAsync<RegisterUserResponse>();

    // Assert - Register
    Assert.Equal(
      HttpStatusCode.Created,
      registerResponse.StatusCode
    );
    Assert.NotNull(registerBody);

    // Act - Login
    var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
    {
      email,
      password
    });

    // Assert - Login
    Assert.Equal(
      HttpStatusCode.OK,
      loginResponse.StatusCode
    );

    var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

    Assert.NotNull(loginBody);
    Assert.False(string.IsNullOrWhiteSpace(loginBody.AccessToken));

    // Act - Me Protected Endpoint
    client.DefaultRequestHeaders.Authorization = new(
      "Bearer",
      loginBody.AccessToken
    );

    var meResponse = await client.GetAsync("/api/auth/me");
    var meBody = await meResponse.Content.ReadFromJsonAsync<GetCurrentUserResponse>();

    // Assert - Me Protected Endpoint
    Assert.Equal(
      HttpStatusCode.OK,
      meResponse.StatusCode
    );
    Assert.NotNull(meBody);
    Assert.Equal(
      registerBody.UserId,
      meBody.UserID
    );
  }

  [Fact]
  public async Task GetMe_ShouldReturnUnauthorized_WhenNoTokenProvided()
  {
    // Arrange
    using var client = _factory.CreateClient();

    // Act
    var meResponse = await client.GetAsync("/api/auth/me");

    // Assert
    Assert.Equal(
      HttpStatusCode.Unauthorized,
      meResponse.StatusCode
    );
  }
}
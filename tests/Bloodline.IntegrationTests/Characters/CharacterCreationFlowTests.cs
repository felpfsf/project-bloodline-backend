using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Bloodline.Api.Features.Characters.CreateCharacter;
using Bloodline.Api.Features.Identity.Login;
using Bloodline.IntegrationTests.Infrastructure;

namespace Bloodline.IntegrationTests.Characters;

[Collection(PostgreSqlCollection.Name)]
public sealed class CharacterCreationFlowTests(PostgreSqlFixture fixture)
{
  private readonly BloodlineWebApplicationFactory _factory = fixture.Factory;

  [Fact]
  public async Task CreateCharacter_ShouldCreateVampire_WhenAuthenticated()
  {
    // Arrange
    using var client = _factory.CreateClient();

    var accessToken = await RegisterAndLoginAsync(client);

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    // Act
    var response = await client.PostAsJsonAsync("api/characters", new
    {
      Name = "Draven",
      Faction = "vampire"
    });

    var body = await response.Content.ReadFromJsonAsync<CreateCharacterResponse>();

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);

    Assert.NotNull(body);
    Assert.NotEqual(Guid.Empty, body!.Id);

    Assert.Equal("Draven", body.Name);
    Assert.Equal("vampire", body.Faction);

    Assert.Equal(1, body.Level);
    Assert.Equal(0, body.Experience);
    Assert.Equal(0, body.Gold);

    Assert.Equal(100, body.Energy);
    Assert.Equal(100, body.MaxEnergy);

    Assert.Equal(10, body.Strength);
    Assert.Equal(9, body.Defense);
    Assert.Equal(12, body.Agility);

    Assert.NotEqual(default, body.CreatedAtUtc);

    Assert.Equal(
      $"/api/characters/{body.Id}",
      response.Headers.Location?.ToString()
    );
  }

  [Fact]
  public async Task CreateCharacter_ShouldReturnUnauthorized_WhenTokenIsMissing()
  {
    // Arrange
    using var client = _factory.CreateClient();

    // Act
    var response = await client.PostAsJsonAsync("api/characters", new
    {
      Name = "Draven",
      Faction = "vampire"
    });

    // Assert
    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [Fact]
  public async Task CreateCharacter_ShouldReturnConflict_WhenUserAlreadyHasCharacter()
  {
    // Arrange
    using var client = _factory.CreateClient();

    var accessToken = await RegisterAndLoginAsync(client);

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    await client.PostAsJsonAsync("api/characters", new
    {
      Name = UniqueCharacterName("Draven"),
      Faction = "vampire"
    });

    // Act
    var duplicateResponse = await client.PostAsJsonAsync("api/characters", new
    {
      Name = UniqueCharacterName("Fenrir"),
      Faction = "werewolf"
    });

    // Assert
    Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
  }

  [Fact]
  public async Task CreateCharacter_ShouldReturnConflict_WhenNameAlreadyExists()
  {
    // Arrange
    using var client = _factory.CreateClient();

    var accessToken = await RegisterAndLoginAsync(client);

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    var characterName = UniqueCharacterName("Draven");

    var firstResponse = await client.PostAsJsonAsync("api/characters", new
    {
      Name = characterName,
      Faction = "vampire"
    });

    // Assert
    Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

    // Arrange - second account tries same name
    using var secondClient = _factory.CreateClient();

    var secondAccessToken = await RegisterAndLoginAsync(secondClient);

    secondClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secondAccessToken);

    var duplicateResponse = await secondClient.PostAsJsonAsync("api/characters", new
    {
      Name = characterName.ToLowerInvariant(),
      Faction = "vampire"
    });

    // Assert
    Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
  }

  [Fact]
  public async Task CreateCharacter_ShouldReturnBadRequest_WhenFactionIsInvalid()
  {
    // Arrange
    using var client = _factory.CreateClient();

    var accessToken = await RegisterAndLoginAsync(client);

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    var invalidFactionResponse = await client.PostAsJsonAsync("api/characters", new
    {
      Name = UniqueCharacterName("Draven"),
      Faction = "alliance"
    });

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, invalidFactionResponse.StatusCode);
  }

  [Fact]
  public async Task CreateCharacter_ShouldReturnBadRequest_WhenNameIsInvalid()
  {
    // Arrange
    using var client = _factory.CreateClient();

    var accessToken = await RegisterAndLoginAsync(client);

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    var invalidNameResponse = await client.PostAsJsonAsync("api/characters", new
    {
      Name = "",
      Faction = "vampire"
    });

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, invalidNameResponse.StatusCode);
  }

  private static string UniqueCharacterName(string prefix) => $"{prefix}{Guid.NewGuid():N}"[..(prefix.Length + 8)];

  private static async Task<string> RegisterAndLoginAsync(HttpClient client)
  {
    var email = $"player{Guid.NewGuid():N}@bloodline.com";
    const string password = "Password123!";

    var registerResponse = await client.PostAsJsonAsync("api/auth/register", new
    {
      Email = email,
      Password = password
    });

    Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

    var loginResponse = await client.PostAsJsonAsync("api/auth/login", new
    {
      Email = email,
      Password = password
    });

    Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

    var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

    Assert.NotNull(loginBody);
    Assert.False(string.IsNullOrWhiteSpace(loginBody!.AccessToken));

    return loginBody.AccessToken;
  }

}
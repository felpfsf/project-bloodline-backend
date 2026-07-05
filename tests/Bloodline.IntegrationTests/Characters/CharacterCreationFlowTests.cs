using System.Net;
using System.Net.Http.Json;
using Bloodline.Api.Features.Characters.CreateCharacter;
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
    using var client = await TestAuthUtils.CreateAuthenticatedClientAsync(_factory);

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
    using var client = await TestAuthUtils.CreateAuthenticatedClientAsync(_factory);

    await client.PostAsJsonAsync("api/characters", new
    {
      Name = TestAuthUtils.UniqueCharacterName("Draven"),
      Faction = "vampire"
    });

    // Act
    var duplicateResponse = await client.PostAsJsonAsync("api/characters", new
    {
      Name = TestAuthUtils.UniqueCharacterName("Fenrir"),
      Faction = "werewolf"
    });

    // Assert
    Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
  }

  [Fact]
  public async Task CreateCharacter_ShouldReturnConflict_WhenNameAlreadyExists()
  {
    // Arrange
    using var client = await TestAuthUtils.CreateAuthenticatedClientAsync(_factory);

    var characterName = TestAuthUtils.UniqueCharacterName("Draven");

    var firstResponse = await client.PostAsJsonAsync("api/characters", new
    {
      Name = characterName,
      Faction = "vampire"
    });

    // Assert
    Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

    // Arrange - second account tries same name
    using var secondClient = await TestAuthUtils.CreateAuthenticatedClientAsync(_factory);

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
    using var client = await TestAuthUtils.CreateAuthenticatedClientAsync(_factory);

    var invalidFactionResponse = await client.PostAsJsonAsync("api/characters", new
    {
      Name = TestAuthUtils.UniqueCharacterName("Draven"),
      Faction = "alliance"
    });

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, invalidFactionResponse.StatusCode);
  }

  [Fact]
  public async Task CreateCharacter_ShouldReturnBadRequest_WhenNameIsInvalid()
  {
    // Arrange
    using var client = await TestAuthUtils.CreateAuthenticatedClientAsync(_factory);

    var invalidNameResponse = await client.PostAsJsonAsync("api/characters", new
    {
      Name = "",
      Faction = "vampire"
    });

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, invalidNameResponse.StatusCode);
  }

}
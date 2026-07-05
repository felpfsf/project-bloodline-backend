using System.Net;
using System.Net.Http.Json;
using Bloodline.Api.Features.Characters.CreateCharacter;
using Bloodline.Api.Features.Characters.GetCurrentCharacter;
using Bloodline.IntegrationTests.Infrastructure;

namespace Bloodline.IntegrationTests.Characters;

[Collection(PostgreSqlCollection.Name)]
public sealed class GetCurrentCharacterFlowTests(PostgreSqlFixture fixture)
{
  private readonly BloodlineWebApplicationFactory _factory = fixture.Factory;

  [Fact]
  public async Task GetCurrentCharacter_Should_Return_Character_When_Authenticated_User_Has_Character()
  {
    using var client = await TestAuthUtils.CreateAuthenticatedClientAsync(_factory);

    var characterName = TestAuthUtils.UniqueCharacterName("Draven");

    var createCharacterResponse = await client.PostAsJsonAsync("api/characters", new
    {
      Name = characterName,
      Faction = "vampire"
    });

    Assert.Equal(HttpStatusCode.Created, createCharacterResponse.StatusCode);

    var createdCharacter = await createCharacterResponse.Content.ReadFromJsonAsync<CreateCharacterResponse>();

    Assert.NotNull(createdCharacter);

    var getCurrentCharacterResponse = await client.GetAsync("api/characters/me");

    var body = await getCurrentCharacterResponse.Content.ReadFromJsonAsync<GetCurrentCharacterResponse>();

    Assert.Equal(HttpStatusCode.OK, getCurrentCharacterResponse.StatusCode);

    Assert.NotNull(body);
    Assert.Equal(createdCharacter.Id, body.Id);
    Assert.Equal(characterName, body.Name);
    Assert.Equal("vampire", body.Faction);
    Assert.Equal(1, body.Level);
    Assert.Equal(100, body.Energy);
  }
  [Fact]
  public async Task GetCurrentCharacter_Should_Return_NotFound_When_Authenticated_User_Does_Not_Have_Character()
  {
    using var client = await TestAuthUtils.CreateAuthenticatedClientAsync(_factory);

    var response = await client.GetAsync("/api/characters/me");

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }
  [Fact]
  public async Task GetCurrentCharacter_Should_Return_Unauthorized_When_User_Token_Is_Missing()
  {
    using var client = _factory.CreateClient();

    var response = await client.GetAsync("/api/characters/me");

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

}
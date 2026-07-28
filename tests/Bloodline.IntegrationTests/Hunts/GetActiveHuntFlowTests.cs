using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Bloodline.Api.Features.Hunts.GetActiveHunt;
using Bloodline.IntegrationTests.Infrastructure;

namespace Bloodline.IntegrationTests.Hunts;

[Collection(PostgreSqlCollection.Name)]
public sealed class GetActiveHuntFlowTests(PostgreSqlFixture fixture)
{

  private readonly BloodlineWebApplicationFactory _factory = fixture.Factory;

  [Fact]
  public async Task GetActiveHunt_Should_Return_Active_Hunt_When_Authenticated_User_Has_Active_Hunt()
  {
    var client = _factory.CreateClient();

    var accessToken = await TestAuthUtils.RegisterLoginAndCreateCharacterAsync(
      client,
      characterName: TestAuthUtils.UniqueCharacterName("ActiveHunt")
    );

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
      "Bearer", accessToken
    );

    var startHuntResponse = await client.PostAsync(
      "api/hunts",
      content: null
    );

    Assert.Equal(HttpStatusCode.Created, startHuntResponse.StatusCode);

    var response = await client.GetAsync(
      "api/hunts/active"
    );

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var body = await response.Content.ReadFromJsonAsync<GetActiveHuntResponse>();

    Assert.NotNull(body);
    Assert.NotNull(body!.ActiveHunt);
    Assert.Equal("shadow_hunt", body.ActiveHunt!.DefinitionCode);
    Assert.Equal(10, body.ActiveHunt.EnergyCost);
    Assert.Equal(25, body.ActiveHunt.ExperienceReward);
    Assert.Equal(15, body.ActiveHunt.GoldReward);
  }

  [Fact]
  public async Task GetActiveHunt_Should_Return_Active_Hunt_When_Authenticated_User_Has_No_Active_Hunt()
  {
    var client = _factory.CreateClient();

    var accessToken = await TestAuthUtils.RegisterLoginAndCreateCharacterAsync(
      client,
      characterName: TestAuthUtils.UniqueCharacterName("Hunt")
    );

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
      "Bearer", accessToken
    );

    var response = await client.GetAsync(
      "api/hunts/active"
    );

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var body = await response.Content.ReadFromJsonAsync<GetActiveHuntResponse>();

    Assert.NotNull(body);
    Assert.Null(body!.ActiveHunt);
  }

  [Fact]
  public async Task GetActiveHunt_Should_Return_Unauthorized_When_Token_Is_Missing()
  {
    var client = _factory.CreateClient();

    var response = await client.GetAsync(
      "api/hunts/active"
    );

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [Fact]
  public async Task GetActiveHunt_Should_Return_CharacterNotFound_When_Authenticated_User_Has_No_Character()
  {
    var client = _factory.CreateClient();

    var accessToken = await TestAuthUtils.RegisterAndLoginAsync(client);

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
      "Bearer", accessToken
    );

    var response = await client.GetAsync(
      "api/hunts/active"
    );

    Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
  }
}
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Bloodline.Api.Features.Hunts.StartHunt;
using Bloodline.IntegrationTests.Infrastructure;

namespace Bloodline.IntegrationTests.Hunts;

[Collection(PostgreSqlCollection.Name)]
public sealed class StartHuntFlowTests(PostgreSqlFixture fixture)
{
  private readonly BloodlineWebApplicationFactory _factory = fixture.Factory;

  [Fact]
  public async Task PostHunts_Should_Start_Hunt_When_Authenticated_User_Has_Character()
  {
    var client = _factory.CreateClient();

    var accessToken = await TestAuthUtils.RegisterLoginAndCreateCharacterAsync(
      client,
      characterName: TestAuthUtils.UniqueCharacterName("Hunt")
    );

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
      "Bearer", accessToken
    );

    var response = await client.PostAsync(
      "api/hunts",
      content: null
    );

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);

    var body = await response.Content.ReadFromJsonAsync<StartHuntResponse>();

    Assert.NotNull(body);

    Assert.NotEqual(Guid.Empty, body!.HuntId);
    Assert.Equal("shadow_hunt", body.DefinitionCode);
    Assert.Equal(10, body.EnergyCost);
    Assert.Equal(25, body.ExperienceReward);
    Assert.Equal(15, body.GoldReward);
    Assert.Equal(90, body.RemainingEnergy);
    Assert.True(body.StartsAtUtc < body.EndsAtUtc);
    Assert.Equal(TimeSpan.FromSeconds(60), body.EndsAtUtc - body.StartsAtUtc);
  }

  [Fact]
  public async Task PostHunts_Should_Return_Unauthorized_When_Token_Is_Missing()
  {
    var client = _factory.CreateClient();

    var response = await client.PostAsync(
      "api/hunts",
      content: null
    );

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [Fact]
  public async Task PostHunts_Should_Return_NotFound_When_Character_Does_Not_Exist()
  {
    var client = _factory.CreateClient();

    var accessToken = await TestAuthUtils.RegisterAndLoginAsync(client);

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
      "Bearer", accessToken
    );

    var response = await client.PostAsync(
      "api/hunts",
      content: null
    );

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

    var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>();

    Assert.NotNull(problem);
    Assert.Equal("character_not_found", problem.Code);
  }

  [Fact]
  public async Task PostHunts_Should_Return_Conflict_When_Character_Has_Already_Active_Hunt()
  {
    var client = _factory.CreateClient();

    var accessToken = await TestAuthUtils.RegisterLoginAndCreateCharacterAsync(
      client,
      characterName: TestAuthUtils.UniqueCharacterName("Hunt")
    );

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
      "Bearer", accessToken
    );

    var firstResponse = await client.PostAsync(
      "api/hunts",
      content: null
    );

    Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

    var secondResponse = await client.PostAsync(
      "api/hunts",
      content: null
    );

    Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);

    var problem = await secondResponse.Content.ReadFromJsonAsync<ProblemDetailsResponse>();

    Assert.NotNull(problem);
    Assert.Equal("hunt_already_active", problem.Code);
  }

  private sealed record ProblemDetailsResponse(
    string? Type,
    string? Title,
    int? Status,
    string? Detail,
    string? Instance,
    string? Code
  );
}